using Btms.Backend.Data;
using Btms.Backend.Data.InMemory;
using Btms.Business.Builders;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Decisions.Finders;
using Btms.Business.Services.Matching;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SlimMessageBus;
using Xunit;
using DecisionContext = Btms.Business.Services.Decisions.DecisionContext;
// ReSharper disable InconsistentNaming

namespace Btms.Business.Tests.Services.Decisions;

public class DecisionServiceTests
{
    [Theory]
    [InlineData(ImportNotificationTypeEnum.Ced, ChedDDecisionCode, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cveda, ChedADecisionCode, "H222")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ChedPDecisionCode, "H222")]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ChedPPDecisionCode, "H219")]
    [InlineData(ImportNotificationTypeEnum.Cvedp, IuuDecisionCode, "H224")]
    public async Task When_processing_decisions_for_ched_type_notifications_not_requiring_iuu_check_Then_should_use_matching_ched_decision_finder_only(ImportNotificationTypeEnum targetImportNotificationType, DecisionCode expectedDecisionCode, params string[] checkCode)
    {
        var decisionContext = CreateDecisionContext(targetImportNotificationType, checkCode, iuuCheckRequired: false);
        var serviceProvider = ConfigureDecisionFinders(decisionContext.Notifications[0], checkCode);
        var decisionService = serviceProvider.GetRequiredService<IDecisionService>();

        var decisionResult = await decisionService.Process(decisionContext, CancellationToken.None);

        decisionResult.Decisions.Should().HaveCount(1);
        decisionResult.Decisions[0].DecisionCode.Should().Be(expectedDecisionCode);
    }

    // [Theory]
    // [InlineData(ImportNotificationTypeEnum.Ced, ChedDDecisionCode)]
    // [InlineData(ImportNotificationTypeEnum.Cveda, ChedADecisionCode)]
    // [InlineData(ImportNotificationTypeEnum.Cvedp, ChedPDecisionCode)]
    // [InlineData(ImportNotificationTypeEnum.Chedpp, ChedPPDecisionCode)]
    // public async Task When_processing_decisions_when_iuu_check_not_indicated_Then_should_use_matching_ched_decision_finder_only(ImportNotificationTypeEnum targetImportNotificationType, DecisionCode expectedDecisionCode, params string[]? checkCode)
    // {
    //     var decisionContext = CreateDecisionContext(targetImportNotificationType, checkCode, iuuCheckRequired: null);
    //     var serviceProvider = ConfigureDecisionFinders(decisionContext.Notifications[0]);
    //     var decisionService = serviceProvider.GetRequiredService<IDecisionService>();
    //
    //     var decisionResult = await decisionService.Process(decisionContext, CancellationToken.None);
    //
    //     decisionResult.Decisions.Should().HaveCount(1);
    //     decisionResult.Decisions[0].DecisionCode.Should().Be(expectedDecisionCode);
    // }

    // [Theory]
    // [InlineData(ImportNotificationTypeEnum.Ced, ChedDDecisionCode)]
    // [InlineData(ImportNotificationTypeEnum.Cveda, ChedADecisionCode)]
    // [InlineData(ImportNotificationTypeEnum.Cvedp, ChedPDecisionCode)]
    // [InlineData(ImportNotificationTypeEnum.Chedpp, ChedPPDecisionCode)]
    // public async Task When_processing_decisions_when_requiring_iuu_check_Then_should_use_matching_ched_decision_and_iuu_decision_finders(ImportNotificationTypeEnum targetImportNotificationType, DecisionCode expectedDecisionCode, params string[]? checkCode)
    // {
    //     var decisionContext = CreateDecisionContext(targetImportNotificationType, checkCode, iuuCheckRequired: true);
    //     var serviceProvider = ConfigureDecisionFinders(decisionContext.Notifications[0]);
    //     var decisionService = serviceProvider.GetRequiredService<IDecisionService>();
    //
    //     var decisionResult = await decisionService.Process(decisionContext, CancellationToken.None);
    //
    //     decisionResult.Decisions.Should().HaveCount(2);
    //     decisionResult.Decisions.Should().Contain(x => x.DecisionCode == expectedDecisionCode && x.DecisionType == DecisionType.Ched);
    //     decisionResult.Decisions.Should().Contain(x => x.DecisionCode == IuuDecisionCode && x.DecisionType == DecisionType.Iuu);
    // }

    [Fact]
    public async Task When_processing_unknown_decisions_Then_should_not_throw()
    {
        var decisionContext = CreateDecisionContext((ImportNotificationTypeEnum)999, ["H224"], iuuCheckRequired: false);
        var serviceProvider = ConfigureDecisionFinders(decisionContext.Notifications[0], ["H224"]);
        var decisionService = serviceProvider.GetRequiredService<IDecisionService>();


        var act = () => decisionService.Process(decisionContext, CancellationToken.None);

        await act.Should().NotThrowAsync<Exception>();
    }

    private const DecisionCode ChedDDecisionCode = DecisionCode.C05;
    private const DecisionCode ChedADecisionCode = DecisionCode.C06;
    private const DecisionCode ChedPDecisionCode = DecisionCode.C07;
    private const DecisionCode ChedPPDecisionCode = DecisionCode.C08;
    private const DecisionCode IuuDecisionCode = DecisionCode.E03;

    private readonly ServiceCollection _serviceCollection = new();

    public DecisionServiceTests()
    {
        _serviceCollection.AddSingleton<IDecisionService, DecisionService>();
        _serviceCollection.AddSingleton(Substitute.For<ILogger<DecisionService>>());
        _serviceCollection.AddSingleton(Substitute.For<IPublishBus>());
        _serviceCollection.AddSingleton<MovementBuilderFactory>();
        _serviceCollection.AddSingleton<DecisionStatusFinder>();
        _serviceCollection.AddLogging();
        _serviceCollection.AddSingleton<IMongoDbContext, MemoryMongoDbContext>();
    }

    private ServiceProvider ConfigureDecisionFinders(ImportNotification notification, string[] checkCodes)
    {
        ConfigureDecisionFinders<ChedDDecisionFinder>(notification, ChedDDecisionCode, checkCodes);
        ConfigureDecisionFinders<ChedADecisionFinder>(notification, ChedADecisionCode, checkCodes);
        ConfigureDecisionFinders<ChedPDecisionFinder>(notification, ChedPDecisionCode, checkCodes);
        ConfigureDecisionFinders<ChedPPDecisionFinder>(notification, ChedPPDecisionCode, checkCodes);
        ConfigureDecisionFinders<IuuDecisionFinder>(notification, IuuDecisionCode, checkCodes);
        return _serviceCollection.BuildServiceProvider();
    }

    private void ConfigureDecisionFinders<T>(ImportNotification notification, DecisionCode expectedDecisionCode, string[] checkCodes) where T : class, IDecisionFinder
    {
        var decisionFinder = Substitute.ForTypeForwardingTo<IDecisionFinder, T>();
        foreach (var checkCode in checkCodes)
            decisionFinder.FindDecision(notification, checkCode).Returns(new DecisionFinderResult(expectedDecisionCode, checkCode));

        _serviceCollection.AddSingleton(decisionFinder);
    }

    private static DecisionContext CreateDecisionContext(ImportNotificationTypeEnum? importNotificationType, string[]? checkCodes, bool? iuuCheckRequired)
    {
        var matchingResult = new MatchingResult();
        matchingResult.AddMatch("notification-1", "movement-1", 1, "document-ref-1");
        return new DecisionContext
        (
            [
                new ImportNotification
                {
                    Id = "notification-1",
                    ImportNotificationType = importNotificationType,
                    Version = 1,
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                    UpdatedEntity = DateTime.Now,
                    CreatedSource = DateTime.Now,
                    UpdatedSource = DateTime.Now,
                    PartTwo = new PartTwo
                    {
                        ControlAuthority = new ControlAuthority
                        {
                            IuuCheckRequired = iuuCheckRequired
                        }
                    }
                }
            ],
            [
                new Movement
                {
                    EntryReference = "movement-1",
                    Id = "movement-1",
                    BtmsStatus = MovementStatus.Default(),
                    Items =
                    [
                        new Items
                        {
                            ItemNumber = 1,
                            Documents =
                            [
                                new Document()
                                {
                                    DocumentCode = "9115"
                                }
                            ],
                            Checks = checkCodes?.Select(checkCode => new Check { CheckCode = checkCode }).ToArray()
                        }
                    ]
                }
            ],
            matchingResult,
            "TestMessageId"
        );
    }
}
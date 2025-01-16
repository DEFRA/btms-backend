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
    [InlineData(ImportNotificationTypeEnum.Ced, ChedDDecisionCode)]
    [InlineData(ImportNotificationTypeEnum.Cveda, ChedADecisionCode)]
    [InlineData(ImportNotificationTypeEnum.Cvedp, ChedPDecisionCode)]
    [InlineData(ImportNotificationTypeEnum.Chedpp, ChedPPDecisionCode)]
    public async Task When_processing_decisions_for_ched_type_notifications_Then_should_use_correct_decision_finder(ImportNotificationTypeEnum targetImportNotificationType, DecisionCode expectedDecisionCode)
    {
        var decisionContext = CreateDecisionContext(targetImportNotificationType);
        var serviceProvider = ConfigureDecisionFinders(decisionContext.Notifications[0]);
        var decisionService = serviceProvider.GetRequiredService<IDecisionService>();

        var decisionResult = await decisionService.Process(decisionContext, CancellationToken.None);

        decisionResult.Decisions[0].DecisionCode.Should().Be(expectedDecisionCode);
    }
    
    [Theory]
    [InlineData(ImportNotificationTypeEnum.Ced)]
    [InlineData(ImportNotificationTypeEnum.Cveda)]
    [InlineData(ImportNotificationTypeEnum.Cvedp)]
    [InlineData(ImportNotificationTypeEnum.Chedpp)]
    public async Task When_processing_decisions_for_iuu_notifications_whatever_the_ched_type_Then_should_use_correct_decision_finder(ImportNotificationTypeEnum targetImportNotificationType)
    {
        var decisionContext = CreateDecisionContext(targetImportNotificationType, true);
        var serviceProvider = ConfigureDecisionFinders(decisionContext.Notifications[0]);
        var decisionService = serviceProvider.GetRequiredService<IDecisionService>();

        var decisionResult = await decisionService.Process(decisionContext, CancellationToken.None);

        decisionResult.Decisions[0].DecisionCode.Should().Be(IuuDecisionCode);
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
    }
    
    private ServiceProvider ConfigureDecisionFinders(ImportNotification notification)
    {
        ConfigureDecisionFinders<ChedDDecisionFinder>(notification, ChedDDecisionCode);
        ConfigureDecisionFinders<ChedADecisionFinder>(notification, ChedADecisionCode);
        ConfigureDecisionFinders<ChedPDecisionFinder>(notification, ChedPDecisionCode);
        ConfigureDecisionFinders<ChedPPDecisionFinder>(notification, ChedPPDecisionCode);
        ConfigureDecisionFinders<IuuDecisionFinder>(notification, IuuDecisionCode);
        return _serviceCollection.BuildServiceProvider();
    }

    private void ConfigureDecisionFinders<T>(ImportNotification notification, DecisionCode expectedDecisionCode) where T : class, IDecisionFinder
    {
        var decisionFinder = Substitute.ForTypeForwardingTo<IDecisionFinder, T>();
        decisionFinder.FindDecision(notification).Returns(new DecisionFinderResult(expectedDecisionCode));
        _serviceCollection.AddSingleton(decisionFinder);
    }

    private static DecisionContext CreateDecisionContext(ImportNotificationTypeEnum? importNotificationType, bool iuuCheckRequired = false)
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
                    Id = "movement-1",
                    BtmsStatus = MovementStatus.Default(),
                    Items =
                    [
                        new Items
                        {
                            ItemNumber = 1,
                            Checks =
                            [
                                new Check()
                            ]
                        }
                    ]
                }
            ],
            matchingResult
        );
    }
}
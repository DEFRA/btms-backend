using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Business.Services.Matching;
using Btms.Business.Services.Validating;
using Btms.Consumers.Extensions;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SlimMessageBus.Host;
using TestDataGenerator;
using Xunit;

namespace Btms.Consumers.Tests;

public class ClearanceRequestConsumerTests
{
    private readonly ILinkingService _mockLinkingService = Substitute.For<ILinkingService>();
    private readonly IDecisionService _decisionService = Substitute.For<IDecisionService>();
    private readonly IMatchingService _matchingService = Substitute.For<IMatchingService>();
    private readonly IValidationService _validationService = Substitute.For<IValidationService>();
    private readonly IMongoDbContext _mongoDbContext = Substitute.For<IMongoDbContext>();
    private readonly IPreProcessor<AlvsClearanceRequest, Movement> _preProcessor = Substitute.For<IPreProcessor<AlvsClearanceRequest, Movement>>();

    [Theory]
    [InlineData(PreProcessingOutcome.New)]
    [InlineData(PreProcessingOutcome.Skipped)]
    [InlineData(PreProcessingOutcome.Changed)]
    [InlineData(PreProcessingOutcome.AlreadyProcessed)]
    public async Task WhenPreProcessingSucceeds_AndLastAuditEntryIsLinked_ThenLinkShouldNotBeRun(PreProcessingOutcome outcome)
    {
        // ARRANGE
        var clearanceRequest = CreateAlvsClearanceRequest();
        var mbFactory = new MovementBuilderFactory(NullLogger<MovementBuilder>.Instance);
        var mb = mbFactory.From(AlvsClearanceRequestMapper.Map(clearanceRequest));
        mb.Update(AuditEntry.CreateLinked("Test", 1));
        var movement = mb.Build();
        _preProcessor.Process(Arg.Any<PreProcessingContext<AlvsClearanceRequest>>())
            .Returns(Task.FromResult(new PreProcessingResult<Movement>(outcome, movement, null)));
        var consumer = CreateSubject(clearanceRequest.Header!.EntryReference!);

        // ACT
        await consumer.OnHandle(clearanceRequest, CancellationToken.None);

        // ASSERT
        consumer.Context.IsLinked().Should().BeFalse();
        await _mockLinkingService.DidNotReceive().Link(Arg.Any<LinkContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenPreProcessingSucceeds_AndLastAuditEntryIsCreated_ThenLinkShouldBeRun()
    {
        // ARRANGE
        var clearanceRequest = CreateAlvsClearanceRequest();
        var mbFactory = new MovementBuilderFactory(NullLogger<MovementBuilder>.Instance);
        var mb = mbFactory.From(AlvsClearanceRequestMapper.Map(clearanceRequest));
        mb.Update(mb.CreateAuditEntry("Test",  CreatedBySystem.Cds));
        var movement = mb.Build();
        _preProcessor.Process(Arg.Any<PreProcessingContext<AlvsClearanceRequest>>())
            .Returns(Task.FromResult(new PreProcessingResult<Movement>(PreProcessingOutcome.New, movement, null)));
        _mockLinkingService.Link(Arg.Any<LinkContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new LinkResult(LinkOutcome.Linked)));
        var consumer = CreateSubject(clearanceRequest.Header!.EntryReference!);

        // ACT
        await consumer.OnHandle(clearanceRequest, CancellationToken.None);

        // ASSERT
        consumer.Context.IsPreProcessed().Should().BeTrue();
        consumer.Context.IsLinked().Should().BeTrue();
        await _mockLinkingService.Received().Link(Arg.Any<LinkContext>(), Arg.Any<CancellationToken>());
    }

    private static AlvsClearanceRequest CreateAlvsClearanceRequest()
    {
        return ClearanceRequestBuilder.Default()
            .WithValidDocumentReferenceNumbers().Build();
    }

    private AlvsClearanceRequestConsumer CreateSubject(string messageId)
    {
        return new AlvsClearanceRequestConsumer(_preProcessor, _mockLinkingService, _matchingService, _decisionService,
            _validationService, _mongoDbContext, NullLogger<AlvsClearanceRequestConsumer>.Instance)
        {
            Context = new ConsumerContext
            {
                Headers = new Dictionary<string, object>
                {
                    { "messageId", messageId }
                }
            }
        };
    }
}

using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Business.Services.Matching;
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
    [Theory]
    [InlineData(PreProcessingOutcome.New)]
    [InlineData(PreProcessingOutcome.Skipped)]
    [InlineData(PreProcessingOutcome.Changed)]
    [InlineData(PreProcessingOutcome.AlreadyProcessed)]
    public async Task WhenPreProcessingSucceeds_AndLastAuditEntryIsLinked_ThenLinkShouldNotBeRun(PreProcessingOutcome outcome)
    {
        // ARRANGE
        var clearanceRequest = CreateAlvsClearanceRequest();
        var movement =
            MovementPreProcessor.BuildMovement(AlvsClearanceRequestMapper.Map(clearanceRequest));

        movement.Update(AuditEntry.CreateLinked("Test", 1));

        var mockLinkingService = Substitute.For<ILinkingService>();
            var decisionService = Substitute.For<IDecisionService>();
            var matchingService = Substitute.For<IMatchingService>();
            var preProcessor = Substitute.For<IPreProcessor<AlvsClearanceRequest, Model.Movement>>();

        preProcessor.Process(Arg.Any<PreProcessingContext<AlvsClearanceRequest>>())
            .Returns(Task.FromResult(new PreProcessingResult<Movement>(outcome, movement, null)));

        var consumer =
                new AlvsClearanceRequestConsumer(preProcessor, mockLinkingService, matchingService, decisionService, NullLogger<AlvsClearanceRequestConsumer>.Instance);
        consumer.Context = new ConsumerContext
        {
            Headers = new Dictionary<string, object>
            {
                { "messageId", clearanceRequest.Header!.EntryReference! }
            }
        };

        // ACT
        await consumer.OnHandle(clearanceRequest);

        // ASSERT
        consumer.Context.IsLinked().Should().BeFalse();

        await mockLinkingService.DidNotReceive().Link(Arg.Any<LinkContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenPreProcessingSucceeds_AndLastAuditEntryIsCreated_ThenLinkShouldBeRun()
    {
        // ARRANGE
        var clearanceRequest = CreateAlvsClearanceRequest();
        var movement =
            MovementPreProcessor.BuildMovement(AlvsClearanceRequestMapper.Map(clearanceRequest));

        movement.Update(AuditEntry.CreateCreatedEntry(movement,"Test", 1, DateTime.Now, AuditEntry.CreatedByCds));

        var mockLinkingService = Substitute.For<ILinkingService>();
            var decisionService = Substitute.For<IDecisionService>();
            var matchingService = Substitute.For<IMatchingService>();
            var preProcessor = Substitute.For<IPreProcessor<AlvsClearanceRequest, Model.Movement>>();

        mockLinkingService.Link(Arg.Any<LinkContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new LinkResult(LinkOutcome.Linked)));

        preProcessor.Process(Arg.Any<PreProcessingContext<AlvsClearanceRequest>>())
            .Returns(Task.FromResult(new PreProcessingResult<Movement>(PreProcessingOutcome.New, movement, null)));

        var consumer =
                new AlvsClearanceRequestConsumer(preProcessor, mockLinkingService, matchingService, decisionService, NullLogger<AlvsClearanceRequestConsumer>.Instance);
        consumer.Context = new ConsumerContext
        {
            Headers = new Dictionary<string, object>
            {
                { "messageId", clearanceRequest.Header!.EntryReference! }
            }
        };

        // ACT
        await consumer.OnHandle(clearanceRequest);

        // ASSERT
        consumer.Context.IsPreProcessed().Should().BeTrue();
        consumer.Context.IsLinked().Should().BeTrue();

        await mockLinkingService.Received().Link(Arg.Any<LinkContext>(), Arg.Any<CancellationToken>());
    }

    private static AlvsClearanceRequest CreateAlvsClearanceRequest()
    {
        return ClearanceRequestBuilder.Default()
            .WithValidDocumentReferenceNumbers().Build();
    }
}
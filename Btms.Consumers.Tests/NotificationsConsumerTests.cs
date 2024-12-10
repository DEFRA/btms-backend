using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Linking;
using Btms.Consumers.Extensions;
using Btms.Model.Auditing;
using Btms.Types.Ipaffs;
using Btms.Types.Ipaffs.Mapping;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SlimMessageBus.Host;
using TestDataGenerator;
using Xunit;

namespace Btms.Consumers.Tests;

public class NotificationsConsumerTests : ConsumerTests
{
    [Theory]
    [InlineData(PreProcessingOutcome.New)]
    [InlineData(PreProcessingOutcome.Skipped)]
    [InlineData(PreProcessingOutcome.Changed)]
    [InlineData(PreProcessingOutcome.AlreadyProcessed)]
    public async Task WhenPreProcessingSucceeds_AndLastAuditEntryIsLinked_ThenLinkShouldNotBeRun(PreProcessingOutcome outcome)
    {
        // ARRANGE
        var notification = CreateImportNotification();
        var modelNotification = notification.MapWithTransform();
        modelNotification.Changed(AuditEntry.CreateLinked("Test", 1, DateTime.Now));
        var mockLinkingService = Substitute.For<ILinkingService>();
        var preProcessor = Substitute.For<IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification>>();

        preProcessor.Process(Arg.Any<PreProcessingContext<ImportNotification>>())
            .Returns(Task.FromResult(new PreProcessingResult<Model.Ipaffs.ImportNotification>(outcome, modelNotification, null)));

        var consumer = new NotificationConsumer(preProcessor, mockLinkingService, NullLogger<NotificationConsumer>.Instance);
        consumer.Context = new ConsumerContext
        {
            Headers = new Dictionary<string, object> { { "messageId", notification.ReferenceNumber! } }
        };

        // ACT
        await consumer.OnHandle(notification);

        // ASSERT
        consumer.Context.IsLinked().Should().BeFalse();

        await mockLinkingService.DidNotReceive().Link(Arg.Any<LinkContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenPreProcessingSucceeds_AndLastAuditEntryIsCreated_ThenLinkShouldBeRun()
    {
        // ARRANGE
        var notification = CreateImportNotification();
        var modelNotification = notification.MapWithTransform();
        modelNotification.Changed(AuditEntry.CreateCreatedEntry(modelNotification, "Test", 1, DateTime.Now));
        var mockLinkingService = Substitute.For<ILinkingService>();
        var preProcessor = Substitute.For<IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification>>();

        mockLinkingService.Link(Arg.Any<LinkContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new LinkResult(LinkOutcome.Linked)));

        preProcessor.Process(Arg.Any<PreProcessingContext<ImportNotification>>())
            .Returns(Task.FromResult(new PreProcessingResult<Model.Ipaffs.ImportNotification>(PreProcessingOutcome.New, modelNotification, null)));


        var consumer = new NotificationConsumer(preProcessor, mockLinkingService, NullLogger<NotificationConsumer>.Instance);
        consumer.Context = new ConsumerContext
        {
            Headers = new Dictionary<string, object> { { "messageId", notification.ReferenceNumber! } }
        };

        // ACT
        await consumer.OnHandle(notification);

        // ASSERT
        consumer.Context.IsPreProcessed().Should().BeTrue();
        consumer.Context.IsLinked().Should().BeTrue();

        await mockLinkingService.Received().Link(Arg.Any<LinkContext>(), Arg.Any<CancellationToken>());
    }

    private static ImportNotification CreateImportNotification()
    {
        return ImportNotificationBuilder.Default()
            .WithReferenceNumber(ImportNotificationTypeEnum.Chedpp, 1, DateTime.UtcNow, 1)
            .WithRandomCommodities(1, 2)
            .Do(x =>
            {
                foreach (var parameterSet in x.PartOne?.Commodities?.ComplementParameterSets!)
                {
                    parameterSet.KeyDataPairs = null;
                }
            }).Build();
    }
}
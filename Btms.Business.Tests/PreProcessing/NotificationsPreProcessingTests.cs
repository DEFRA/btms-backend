using Btms.Backend.Data.InMemory;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Types.Ipaffs;
using Btms.Types.Ipaffs.Mapping;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TestDataGenerator;
using Xunit;

namespace Btms.Business.Tests.PreProcessing;

public class NotificationsPreProcessingTests
{
    [Fact]
    public async Task WhenNotificationNotExists_ThenShouldBeCreated()
    {
        // ARRANGE
        var notification = CreateImportNotification();
        var dbContext = new MemoryMongoDbContext();
        var preProcessor = new ImportNotificationPreProcessor(dbContext, NullLogger<ImportNotificationPreProcessor>.Instance);

        // ACT
        var preProcessingResult = await preProcessor.Process(
            new PreProcessingContext<ImportNotification>(notification, "TestMessageId"));

        // ASSERT
        preProcessingResult.Outcome.Should().Be(PreProcessingOutcome.New);
        var savedNotification = await dbContext.Notifications.Find(notification.ReferenceNumber!);
        savedNotification.Should().NotBeNull();
        savedNotification?.AuditEntries.Count.Should().Be(1);
        savedNotification?.AuditEntries[0].Status.Should().Be("Created");
        savedNotification?.Updated.Should().BeAfter(default);
    }

    [Fact]
    public async Task WhenNotificationExists_AndLastUpdatedIsNewer_ThenShouldBeUpdated()
    {
        // ARRANGE
        var notification = CreateImportNotification();
        var dbContext = new MemoryMongoDbContext();
        await dbContext.Notifications.Insert(notification.MapWithTransform());
        notification.LastUpdated = notification.LastUpdated?.AddHours(1);
        var preProcessor = new ImportNotificationPreProcessor(dbContext, NullLogger<ImportNotificationPreProcessor>.Instance);

        // ACT
        var preProcessingResult = await preProcessor.Process(
            new PreProcessingContext<ImportNotification>(notification, "TestMessageId"));

        // ASSERT
        preProcessingResult.Outcome.Should().Be(PreProcessingOutcome.Changed);
        var savedNotification = await dbContext.Notifications.Find(notification.ReferenceNumber!);
        savedNotification.Should().NotBeNull();
        savedNotification?.AuditEntries.Count.Should().Be(1);
        savedNotification?.AuditEntries[0].Status.Should().Be("Updated");
        savedNotification?.Updated.Should().BeAfter(default);
    }

    [Theory]
    [InlineData(ImportNotificationStatusEnum.Validated, Model.Ipaffs.ImportNotificationStatusEnum.Validated)]
    [InlineData(ImportNotificationStatusEnum.Rejected, Model.Ipaffs.ImportNotificationStatusEnum.Rejected)]
    [InlineData(ImportNotificationStatusEnum.PartiallyRejected, Model.Ipaffs.ImportNotificationStatusEnum.PartiallyRejected)]

    [InlineData(ImportNotificationStatusEnum.Submitted, Model.Ipaffs.ImportNotificationStatusEnum.InProgress)]
    [InlineData(ImportNotificationStatusEnum.Amend, Model.Ipaffs.ImportNotificationStatusEnum.InProgress)]
    [InlineData(ImportNotificationStatusEnum.Modify, Model.Ipaffs.ImportNotificationStatusEnum.InProgress)]
    [InlineData(ImportNotificationStatusEnum.Replaced, Model.Ipaffs.ImportNotificationStatusEnum.InProgress)]
    public async Task WhenNotificationExists_AndLastUpdatedIsNewer_AndStatusInGoingBackToInProgress_ThenShouldNotBeUpdated(ImportNotificationStatusEnum status, Model.Ipaffs.ImportNotificationStatusEnum expectedStatus)
    {
        // ARRANGE
        var notification = CreateImportNotification(status);
        var dbContext = new MemoryMongoDbContext();
        await dbContext.Notifications.Insert(notification.MapWithTransform());
        notification.LastUpdated = notification.LastUpdated?.AddHours(1);
        notification.Status = ImportNotificationStatusEnum.InProgress;
        var preProcessor = new ImportNotificationPreProcessor(dbContext, NullLogger<ImportNotificationPreProcessor>.Instance);

        // ACT
        var preProcessingResult = await preProcessor.Process(
            new PreProcessingContext<ImportNotification>(notification, "TestMessageId"));

        // ASSERT
        preProcessingResult.Outcome.Should().Be((int)status == (int)expectedStatus ? PreProcessingOutcome.Skipped : PreProcessingOutcome.Changed);
        var savedNotification = await dbContext.Notifications.Find(notification.ReferenceNumber!);
        savedNotification?.Status.Should().Be(expectedStatus);
        savedNotification.Should().NotBeNull();
        savedNotification?.AuditEntries.Count.Should().Be(preProcessingResult.Outcome == PreProcessingOutcome.Skipped ? 0 : 1);
    }

    private static ImportNotification CreateImportNotification(ImportNotificationStatusEnum status = ImportNotificationStatusEnum.Submitted)
    {
        return ImportNotificationBuilder.Default()
            .WithReferenceNumber(ImportNotificationTypeEnum.Chedpp, 1, DateTime.UtcNow, 1)
            .WithRandomCommodities(1, 2)
            .Do(x =>
            {
                x.Status = status;
                foreach (var parameterSet in x.PartOne?.Commodities?.ComplementParameterSets!)
                {
                    parameterSet.KeyDataPairs = null;
                }
            }).Build();
    }
}
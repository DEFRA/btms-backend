using Btms.Backend.Data;
using Btms.Backend.Data.InMemory;
using Btms.Business.Services.Linking;
using Btms.Metrics;
using Btms.Model.Cds;
using Btms.Model.ChangeLog;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Document = Btms.Model.Cds.Document;
using Items = Btms.Model.Cds.Items;
using Movement = Btms.Model.Movement;

namespace Btms.Business.Tests.Services.Linking;

public class LinkingServiceTests
{
    private static readonly Random Random = new();
    protected readonly IMongoDbContext dbContext = new MemoryMongoDbContext();
    private readonly LinkingMetrics linkingMetrics = new(new DummyMeterFactory());
    private static string GenerateDocumentReference(string id) => $"GBCVD2024.{id}";
    private static string GenerateNotificationReference(string id) => $"CHEDP.GB.2024.{id}";

    [Fact]
    public async Task Link_UnknownContextType_ShouldThrowException()
    {
        // Arrange
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var ctx = new BadContext();

        // Act
        var test = () => sut.Link(ctx);

        // Assert
        await test.Should().ThrowAsync<LinkException>();
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_AddedInitialDocuments_AddsNotificationLinks()
    {
        // Arrange
        var testData = await AddTestData(3, 1, 0);
        var beforeLinkChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContextWithDocuments(testData.Movements[0], [], [testData.Cheds[0], testData.Cheds[1]]);

        // Act
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(2);
        linkResult.Movements.Count.Should().Be(1);

        var afterChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // added cheds
        beforeLinkChedRelCounts[0].Should().Be(0);
        afterChedRelCounts[0].Should().Be(1);
        beforeLinkChedRelCounts[1].Should().Be(0);
        afterChedRelCounts[1].Should().Be(1);

        // unlinked ched
        beforeLinkChedRelCounts[2].Should().Be(0);
        afterChedRelCounts[2].Should().Be(0);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_AddedInitialItems_AddsNotificationLinks()
    {
        // Arrange
        var testData = await AddTestData(3, 1, 0);
        var beforeLinkChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContextWithItems(testData.Movements[0], [], [testData.Cheds[0], testData.Cheds[1]]);

        // Act
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(2);
        linkResult.Movements.Count.Should().Be(1);

        var afterChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // added cheds
        beforeLinkChedRelCounts[0].Should().Be(0);
        afterChedRelCounts[0].Should().Be(1);
        beforeLinkChedRelCounts[1].Should().Be(0);
        afterChedRelCounts[1].Should().Be(1);

        // unlinked ched
        beforeLinkChedRelCounts[2].Should().Be(0);
        afterChedRelCounts[2].Should().Be(0);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_AddedSomeDocuments_AddsNotificationLinks()
    {
        // Arrange
        var testData = await AddTestData(3, 1, 0);
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);

        // establish existing links
        var movementCtx = CreateMovementContextWithDocuments(testData.Movements[0], [], [testData.Cheds[0]]);
        var prelink = await sut.Link(movementCtx);
        var beforeLinkChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // Act
        movementCtx = CreateMovementContextWithDocuments(prelink.Movements[0], [testData.Cheds[0]], [testData.Cheds[0], testData.Cheds[1]]);
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(2);
        linkResult.Movements.Count.Should().Be(1);

        var afterChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // unchanged ched
        beforeLinkChedRelCounts[0].Should().Be(1);
        afterChedRelCounts[0].Should().Be(1);

        // added ched
        beforeLinkChedRelCounts[1].Should().Be(0);
        afterChedRelCounts[1].Should().Be(1);

        // unlinked ched
        beforeLinkChedRelCounts[2].Should().Be(0);
        afterChedRelCounts[2].Should().Be(0);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_AddedSomeItems_AddsNotificationLinks()
    {
        // Arrange
        var testData = await AddTestData(3, 1, 0);
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);

        // establish existing links
        var movementCtx = CreateMovementContextWithItems(testData.Movements[0], [], [testData.Cheds[0]]);
        var prelink = await sut.Link(movementCtx);
        var beforeLinkChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // Act
        movementCtx = CreateMovementContextWithItems(prelink.Movements[0], [testData.Cheds[0]], [testData.Cheds[0], testData.Cheds[1]]);
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(2);
        linkResult.Movements.Count.Should().Be(1);

        var afterChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // unchanged ched
        beforeLinkChedRelCounts[0].Should().Be(1);
        afterChedRelCounts[0].Should().Be(1);

        // added ched
        beforeLinkChedRelCounts[1].Should().Be(0);
        afterChedRelCounts[1].Should().Be(1);

        // unlinked ched
        beforeLinkChedRelCounts[2].Should().Be(0);
        afterChedRelCounts[2].Should().Be(0);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_ChangedSomeDocuments_RemovesAndAddsNotificationLinks()
    {
        // Arrange
        var testData = await AddTestData(3, 1, 0);
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);

        // establish existing links
        var movementCtx = CreateMovementContextWithDocuments(testData.Movements[0], [], [testData.Cheds[0]]);
        var prelink = await sut.Link(movementCtx);
        var beforeLinkChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // Act
        movementCtx = CreateMovementContextWithDocuments(prelink.Movements[0], [testData.Cheds[0]], [testData.Cheds[1]]);
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(1);

        var afterChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // removed cheds
        beforeLinkChedRelCounts[0].Should().Be(1);
        afterChedRelCounts[0].Should().Be(0);

        // added cheds
        beforeLinkChedRelCounts[1].Should().Be(0);
        afterChedRelCounts[1].Should().Be(1);

        // unlinked ched
        beforeLinkChedRelCounts[2].Should().Be(0);
        afterChedRelCounts[2].Should().Be(0);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_ChangedSomeItems_RemovesAndAddsNotificationLinks()
    {
        // Arrange
        var testData = await AddTestData(3, 1, 0);
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);

        // establish existing links
        var movementCtx = CreateMovementContextWithItems(testData.Movements[0], [], [testData.Cheds[0]]);
        var prelink = await sut.Link(movementCtx);
        var beforeLinkChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // Act
        movementCtx = CreateMovementContextWithItems(prelink.Movements[0], [testData.Cheds[0]], [testData.Cheds[1]]);
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(1);

        var afterChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // removed cheds
        beforeLinkChedRelCounts[0].Should().Be(1);
        afterChedRelCounts[0].Should().Be(0);

        // added cheds
        beforeLinkChedRelCounts[1].Should().Be(0);
        afterChedRelCounts[1].Should().Be(1);

        // unlinked ched
        beforeLinkChedRelCounts[2].Should().Be(0);
        afterChedRelCounts[2].Should().Be(0);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_MatchingCHEDs_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData();
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContext(testData.Movements[0], [testData.Cheds[0], null], true, false);

        // Act
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(1);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_NoMatchingCHEDs_NoMatchingTriggered()
    {
        // Arrange
        var testData = await AddTestData();
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContext(testData.Movements[0], [null], true, false);

        // Act
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.NotLinked);
        linkResult.Notifications.Count.Should().Be(0);
        linkResult.Movements.Count.Should().Be(1);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_RemovedSomeDocuments_RemovesNotificationLinks()
    {
        // Arrange
        var testData = await AddTestData(4, 1, 0);
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);

        // establish existing links
        var movementCtx = CreateMovementContextWithDocuments(testData.Movements[0], [], [testData.Cheds[0], testData.Cheds[1], testData.Cheds[2]]);
        var prelink = await sut.Link(movementCtx);
        var beforeLinkChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // Act
        movementCtx = CreateMovementContextWithDocuments(prelink.Movements[0], [testData.Cheds[0], testData.Cheds[1], testData.Cheds[2]], [testData.Cheds[0]]);
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(1);

        var afterChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // unchanged ched
        beforeLinkChedRelCounts[0].Should().Be(1);
        afterChedRelCounts[0].Should().Be(1);

        // removed cheds
        beforeLinkChedRelCounts[1].Should().Be(1);
        afterChedRelCounts[1].Should().Be(0);
        beforeLinkChedRelCounts[2].Should().Be(1);
        afterChedRelCounts[2].Should().Be(0);

        // unlinked ched
        beforeLinkChedRelCounts[3].Should().Be(0);
        afterChedRelCounts[3].Should().Be(0);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_RemovedSomeItems_RemovesNotificationLinks()
    {
        // Arrange
        var testData = await AddTestData(4, 1, 0);
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);

        // establish existing links
        var movementCtx = CreateMovementContextWithItems(testData.Movements[0], [], [testData.Cheds[0], testData.Cheds[1], testData.Cheds[2]]);
        var prelink = await sut.Link(movementCtx);
        var beforeLinkChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // Act
        movementCtx = CreateMovementContextWithItems(prelink.Movements[0], [testData.Cheds[0], testData.Cheds[1], testData.Cheds[2]], [testData.Cheds[0]]);
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(1);

        var afterChedRelCounts = testData.Cheds.Select(c =>
            dbContext.Notifications.Single(x => x.Id == c.Id).Relationships.Movements.Data.Count).ToList();

        // unchanged ched
        beforeLinkChedRelCounts[0].Should().Be(1);
        afterChedRelCounts[0].Should().Be(1);

        // removed cheds
        beforeLinkChedRelCounts[1].Should().Be(1);
        afterChedRelCounts[1].Should().Be(0);
        beforeLinkChedRelCounts[2].Should().Be(1);
        afterChedRelCounts[2].Should().Be(0);

        // unlinked ched
        beforeLinkChedRelCounts[3].Should().Be(0);
        afterChedRelCounts[3].Should().Be(0);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_NoFieldsOfInterest_NoMatchingTriggered()
    {
        // Arrange
        var testData = await AddTestData();

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContext(testData.Movements[0], [testData.Cheds[0]], true, true);

        // Act
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.LinksExist);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(1);
    }

    [Fact]
    public async Task LinkMovement_NewRequest_MatchingCHED_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData();

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContext(null, [testData.Cheds[0]], true, false);

        // Act
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(1);
    }

    [Fact]
    public async Task LinkMovement_NewRequest_MultipleMatchingCHEDs_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData(2, 1, 2);

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContext(null, [testData.Cheds[0], testData.Cheds[1]], false, true);

        // Act
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(2);
        linkResult.Movements.Count.Should().Be(1);
    }

    [Fact]
    public async Task LinkMovement_NewRequest_NoMatchingCHEDs_NoMatchingTriggered()
    {
        // Arrange
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContext(null, [null], true, false);

        // Act
        var linkResult = await sut.Link(movementCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.NotLinked);
        linkResult.Notifications.Count.Should().Be(0);
        linkResult.Movements.Count.Should().Be(1);
    }

    [Fact]
    public async Task LinkNotification_ExistingNotification_IncludesFieldsOfInterest_MatchingMRN_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData();

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var notificationCtx = CreateNotificationContext(testData.Cheds[0], true, true);

        // Act
        var linkResult = await sut.Link(notificationCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(1);
    }

    [Fact]
    public async Task LinkNotification_ExistingNotification_IncludesFieldsOfInterest_MultipleMatchingMRNs_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData(2, 2, 2);

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var notificationCtx = CreateNotificationContext(testData.Cheds[0], true, true);

        // Act
        var linkResult = await sut.Link(notificationCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(2);
    }

    [Fact]
    public async Task LinkNotification_ExistingNotification_IncludesFieldsOfInterest_NoMatchingMRNs_NoMatchingTriggered()
    {
        // Arrange
        var testData = await AddTestData(1, 1, 0);

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var notificationCtx = CreateNotificationContext(testData.Cheds[0], true, true);

        // Act
        var linkResult = await sut.Link(notificationCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.NotLinked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(0);
    }

    [Fact]
    public async Task LinkNotification_ExistingNotification_NoFieldsOfInterest_NoMatchingTriggered()
    {
        // Arrange
        var testData = await AddTestData(2, 2, 2);

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var notificationCtx = CreateNotificationContext(testData.Cheds[0], true, false);

        // Act
        var linkResult = await sut.Link(notificationCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.LinksExist);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(2);
    }

    [Fact]
    public async Task LinkNotification_NewNotification_MatchingMRN_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData(0, 4, 0, 1);

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var notificationCtx = CreateNotificationContext(testData.UnmatchedChedRefs[0], string.Empty, false, true);

        // Act
        var linkResult = await sut.Link(notificationCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(4);
    }

    [Fact]
    public async Task LinkNotification_NewNotification_MultipleMatchingMRNs_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData(0, 1, 0, 1);

        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var notificationCtx = CreateNotificationContext(testData.UnmatchedChedRefs[0], string.Empty, false, true);

        // Act
        var linkResult = await sut.Link(notificationCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.Linked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(1);
    }

    [Fact]
    public async Task LinkNotification_NewNotification_NoMatchingMRNs_NoMatchingTriggered()
    {
        // Arrange
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var notificationCtx = CreateNotificationContext(null, false, false);

        // Act
        var linkResult = await sut.Link(notificationCtx);

        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.NotLinked);
        linkResult.Notifications.Count.Should().Be(1);
        linkResult.Movements.Count.Should().Be(0);
    }

    protected LinkingService CreateLinkingService()
    {
        return new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
    }

    protected MovementLinkContext CreateMovementContextWithItems(Movement? movement, List<ImportNotification?> existingCheds, List<ImportNotification?> receivedCheds)
    {
        var entryReference = movement != null ? movement.EntryReference : $"TEST{GenerateRandomReference()}";
        var etag = movement != null ? movement._Etag : string.Empty;
        var existingChedReferences = existingCheds
            .Select(x => x != null ? x._MatchReference : $"{GenerateRandomReference()}")
            .ToList();
        var receivedChedReferences = receivedCheds
            .Select(x => x != null ? x._MatchReference : $"{GenerateRandomReference()}")
            .ToList();

        var mov = new Movement
        {
            Id = entryReference,
            EntryReference = entryReference,
            _Etag = etag,
            Items = receivedChedReferences.Select(x => new Items
            {
                Documents = [new Document { DocumentReference = GenerateDocumentReference(x), DocumentCode = "C640" }]
            }).ToList(),
            ClearanceRequests = new(),
            Status = MovementStatus.Default()
        };

        var existingMovement = new Movement
        {
            Id = entryReference,
            EntryReference = entryReference,
            Items = existingChedReferences.Select(x => new Items
            {
                Documents = [new Document { DocumentReference = GenerateDocumentReference(x), DocumentCode = "C640" }]
            }).ToList(),
            ClearanceRequests = new(),
            Status = MovementStatus.Default()
        };

        var changeSet = mov.GenerateChangeSet(existingMovement);
        var output = LinkContext.ForMovement(mov, changeSet);

        return output;
    }

    protected MovementLinkContext CreateMovementContextWithDocuments(Movement? movement, List<ImportNotification?> existingCheds, List<ImportNotification?> receivedCheds)
    {
        var entryReference = movement != null ? movement.EntryReference : $"TEST{GenerateRandomReference()}";
        var etag = movement != null ? movement._Etag : string.Empty;
        var existingChedReferences = existingCheds
            .Select(x => x != null ? x._MatchReference : $"{GenerateRandomReference()}")
            .ToList();
        var receivedChedReferences = receivedCheds
            .Select(x => x != null ? x._MatchReference : $"{GenerateRandomReference()}")
           .ToList();

        var mov = new Movement
        {
            Id = entryReference,
            EntryReference = entryReference,
            _Etag = etag,
            Items = [
                new Items
                {
                    Documents = receivedChedReferences
                        .Select(x => new Document { DocumentReference = GenerateDocumentReference(x), DocumentCode = "C640" })
                        .ToArray()
                }],
            ClearanceRequests = new(),
            Status = MovementStatus.Default()
        };

        var existingMovement = new Movement
        {
            Id = entryReference,
            EntryReference = entryReference,
            Items = [
                new Items
                {
                    Documents = existingChedReferences
                        .Select(x => new Document { DocumentReference = GenerateDocumentReference(x), DocumentCode = "C640" })
                        .ToArray()
                }],
            ClearanceRequests = new(),
            Status = MovementStatus.Default()
        };

        var changeSet = mov.GenerateChangeSet(existingMovement);
        var output = LinkContext.ForMovement(mov, changeSet);

        return output;
    }


    protected MovementLinkContext CreateMovementContext(Movement? movement, List<ImportNotification?> cheds, bool createExistingMovement, bool existingDocs, bool newDocs = true)
    {
        var entryReference = movement != null ? movement.EntryReference : $"TEST{GenerateRandomReference()}";
        var etag = movement != null ? movement._Etag : string.Empty;
        var chedReferences = cheds
            .Select(x => x != null ? x._MatchReference : $"{GenerateRandomReference()}")
            .ToList();

        var mov = new Movement
        {
            Id = entryReference,
            EntryReference = entryReference,
            _Etag = etag,
            Items = chedReferences.Select(x => new Items
            {
                Documents = newDocs
                ? [new Document { DocumentReference = GenerateDocumentReference(x), DocumentCode = "C640" }]
                : []
            }).ToList(),
            ClearanceRequests = new(),
            Status = MovementStatus.Default()
        };

        var existingMovement = createExistingMovement ?
            new Movement
            {
                Id = entryReference,
                EntryReference = entryReference,
                Items = chedReferences.Select(x => new Items
                {
                    Documents = existingDocs
                        ? [new Document { DocumentReference = GenerateDocumentReference(x), DocumentCode = "C640" }]
                        : []
                }).ToList(),
                ClearanceRequests = new(),
                Status = MovementStatus.Default()
            } : null;


        var changeSet = mov.GenerateChangeSet(existingMovement);
        var output = LinkContext.ForMovement(mov, createExistingMovement ? changeSet : null);

        return output;
    }

    protected ImportNotificationLinkContext CreateNotificationContext(ImportNotification? ched,
        bool createExistingNotification, bool fieldsOfInterest)
    {
        var chedReference = ched != null ? ched._MatchReference : GenerateRandomReference();
        var etag = ched != null ? ched._Etag : string.Empty;

        return CreateNotificationContext(chedReference, etag, createExistingNotification, fieldsOfInterest);
    }

    protected ImportNotificationLinkContext CreateNotificationContext(string chedReference, string etag, bool createExistingNotification, bool fieldsOfInterest)
    {
        var notification = new ImportNotification
        {
            Id = GenerateNotificationReference(chedReference),
            UpdatedEntity = DateTime.UtcNow,
            _Etag = etag,
            Commodities =
                [new CommodityComplement { CommodityId = "1234567", CommodityDescription = "Definitely real things" }]
        };

        CommodityComplement[] c = fieldsOfInterest
            ? []
            : [new CommodityComplement { CommodityId = "1234567", CommodityDescription = "Definitely real things" }];

        var existingNotification = createExistingNotification
            ? new ImportNotification
            {
                Id = GenerateNotificationReference(chedReference),
                UpdatedEntity = DateTime.UtcNow,
                Commodities = c
            }
            : null;

        var changeSet = notification.GenerateChangeSet(existingNotification);
        var output = LinkContext.ForImportNotification(notification, createExistingNotification ? changeSet : null);

        return output;
    }

    protected async Task<(List<ImportNotification> Cheds, List<Movement> Movements, List<string> UnmatchedChedRefs)> AddTestData(int chedCount = 1, int movementCount = 1, int matchedChedsPerMovement = 1, int unMatchedChedsPerMovement = 0)
    {
        matchedChedsPerMovement = int.Min(matchedChedsPerMovement, chedCount);
        var movements = new List<Movement>();
        var cheds = new List<ImportNotification>();

        var unmatchedChedRefs = Enumerable
            .Range(0, unMatchedChedsPerMovement)
            .Select(_ => GenerateRandomReference()).ToList();

        for (var i = 0; i < chedCount; i++)
        {
            var matchingRef = GenerateRandomReference();
            var ched = new ImportNotification
            {
                UpdatedEntity = DateTime.UtcNow.AddHours(-1),
                ReferenceNumber = GenerateNotificationReference(matchingRef),
                Commodities = []
            };

            cheds.Add(ched);

            await dbContext.Notifications.Insert(ched);
        }

        for (var i = 0; i < movementCount; i++)
        {
            var entryRef = $"TESTREF{GenerateRandomReference()}";
            var mov = new Movement
            {
                Id = entryRef,
                EntryReference = entryRef,
                ClearanceRequests =
                [
                    new CdsClearanceRequest
                    {
                        Header = new() { EntryReference = entryRef, EntryVersionNumber = 3, DeclarationType = "F" }
                    }
                ],
                Items = new(),
                Status = MovementStatus.Default()
            };

            movements.Add(mov);

            for (var j = 0; j < matchedChedsPerMovement; j++)
            {
                var matchRef = cheds[j]._MatchReference;
                var refNo = matchRef;

                mov.Items.Add(
                    new Items
                    {
                        Documents =
                        [
                            new Document { DocumentReference = GenerateDocumentReference(refNo), DocumentCode = "C640" }
                        ]
                    });
            }

            foreach (var refNo in unmatchedChedRefs)
            {
                mov.Items.Add(
                    new Items
                    {
                        Documents =
                        [
                            new Document { DocumentReference = GenerateDocumentReference(refNo), DocumentCode = "C640" }
                        ]
                    });
            }

            await dbContext.Movements.Insert(mov);
        }

        return (cheds, movements, unmatchedChedRefs);
    }

    private static string GenerateRandomReference()
    {
        var intString = "1";

        for (var i = 0; i < 6; i++)
        {
            intString += Random.Next(9).ToString();
        }

        return $"{intString}";
    }
}

public record BadContext : LinkContext
{
    public override string GetIdentifiers()
    {
        return "Test";
    }
}
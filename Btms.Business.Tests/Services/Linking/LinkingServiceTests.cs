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
    private static readonly Random Random = new ();
    private readonly IMongoDbContext dbContext = new MemoryMongoDbContext();
    private readonly LinkingMetrics linkingMetrics = new(new DummyMeterFactory());
    private static string GenerateDocumentReference(int id) => $"GBCVD2024.{id}";
    private static string GenerateNotificationReference(int id) => $"CHEDP.GB.2024.{id}";

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
    public async Task LinkMovement_ExistingRequest_IncludesFieldsOfInterest_MatchingCHEDs_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData();
        
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContext(testData.Movements[0], [testData.Cheds[0], null], true, true);

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
        var movementCtx = CreateMovementContext(testData.Movements[0], [null], true, true);
        
        // Act
        var linkResult = await sut.Link(movementCtx);
        
        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.NotLinked);
        linkResult.Notifications.Count.Should().Be(0);
        linkResult.Movements.Count.Should().Be(1);
    }

    [Fact]
    public async Task LinkMovement_ExistingRequest_NoFieldsOfInterest_NoMatchingTriggered()
    {
        // Arrange
        var testData = await AddTestData();
        
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContext(testData.Movements[0], [testData.Cheds[0]], true, false);
        
        // Act
        var linkResult = await sut.Link(movementCtx);
        
        // Assert
        linkResult.Should().NotBeNull();
        linkResult.Outcome.Should().Be(LinkOutcome.NotLinked);
        linkResult.Notifications.Count.Should().Be(0);
        linkResult.Movements.Count.Should().Be(0);
    }

    [Fact]
    public async Task LinkMovement_NewRequest_MatchingCHED_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData();
        
        var sut = new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
        var movementCtx = CreateMovementContext(null, [testData.Cheds[0]], true, true);
        
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
        var movementCtx = CreateMovementContext(null, [testData.Cheds[0], testData.Cheds[1]], false, false);
        
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
        var movementCtx = CreateMovementContext(null, [null], true, true);
        
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
        var testData = await AddTestData(2,2,2);
        
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
        linkResult.Outcome.Should().Be(LinkOutcome.NotLinked);
        linkResult.Notifications.Count.Should().Be(0);
        linkResult.Movements.Count.Should().Be(0);
    }

    [Fact]
    public async Task LinkNotification_NewNotification_MatchingMRN_AddsAllToLinkResult()
    {
        // Arrange
        var testData = await AddTestData(0,4,0, 1);
        
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
        var testData = await AddTestData(0,1,0, 1);
        
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

    private MovementLinkContext CreateMovementContext(Movement? movement, List<ImportNotification?> cheds, bool createExistingMovement, bool fieldsOfInterest)
    {
        var entryReference = movement != null ? movement.EntryReference : $"TEST{GenerateRandomReference()}";
        var etag = movement != null ? movement._Etag : string.Empty;
        var chedReferences = cheds
            .Select(x => x != null ? x._MatchReference : $"{GenerateRandomReference()}")
            .Select(y => int.Parse(y)).ToList();
        
        var mov = new Movement
        {
            Id = entryReference,
            EntryReference = entryReference,
            _Etag = etag,
            Items = chedReferences.Select(x => new Items
            {
                Documents = [ new Document { DocumentReference = GenerateDocumentReference(x) } ]
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
                    Documents = fieldsOfInterest
                        ? []
                        : [ new Document { DocumentReference = GenerateDocumentReference(x) } ]
                }).ToList(),
                ClearanceRequests = new(),
                Status = MovementStatus.Default()
            } : null;


        var changeSet = mov.GenerateChangeSet(existingMovement);
        var output = LinkContext.ForMovement(mov, createExistingMovement ? changeSet : null);
        
        return output;
    }

    private ImportNotificationLinkContext CreateNotificationContext(ImportNotification? ched,
        bool createExistingNotification, bool fieldsOfInterest)
    {
        var chedReference = ched != null ? int.Parse(ched._MatchReference) : GenerateRandomReference();
        var etag = ched != null ? ched._Etag : string.Empty;

        return CreateNotificationContext(chedReference, etag, createExistingNotification, fieldsOfInterest);
    }
    
    private ImportNotificationLinkContext CreateNotificationContext(int chedReference, string etag, bool createExistingNotification, bool fieldsOfInterest)
    {
        var notification = new ImportNotification
        {
            Id = GenerateNotificationReference(chedReference),
            Updated = DateTime.UtcNow,
            _Etag = etag,
            Commodities =
                [ new CommodityComplement { CommodityId = "1234567", CommodityDescription = "Definitely real things" }]
        };

        CommodityComplement[] c = fieldsOfInterest
            ? []
            : [new CommodityComplement { CommodityId = "1234567", CommodityDescription = "Definitely real things" }];

        var existingNotification = createExistingNotification
            ? new ImportNotification
            {
                Id = GenerateNotificationReference(chedReference),
                Updated = DateTime.UtcNow,
                Commodities = c
            }
            : null;

        var changeSet = notification.GenerateChangeSet(existingNotification);
        var output = LinkContext.ForImportNotification(notification, createExistingNotification ? changeSet : null);

        return output;
    }
    
    private async Task<(List<ImportNotification> Cheds, List<Movement> Movements, List<int> UnmatchedChedRefs)> AddTestData(int chedCount = 1, int movementCount = 1, int matchedChedsPerMovement = 1,  int unMatchedChedsPerMovement = 0)
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
                Updated = DateTime.UtcNow.AddHours(-1),
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
                Items = new (),
                Status = MovementStatus.Default()
            };
            
            movements.Add(mov);
            
            for (var j = 0; j < matchedChedsPerMovement; j++)
            {
                var matchRef = cheds[j]._MatchReference;
                var refNo = int.Parse(matchRef);
                
                mov.Items.Add(
                    new Items
                    {
                        Documents =
                        [
                            new Document { DocumentReference = GenerateDocumentReference(refNo) }
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
                            new Document { DocumentReference = GenerateDocumentReference(refNo) }
                        ]
                    });
            }

            await dbContext.Movements.Insert(mov);
        }
        
        return (cheds, movements, unmatchedChedRefs);
    }
    
    private static int GenerateRandomReference()
    {
        var intString = "1";
        
        for (var i = 0; i < 6; i++)
        {
            intString += Random.Next(9).ToString();
        }
        
        return int.Parse(intString);
    }
}

public record BadContext : LinkContext
{
    public override string GetIdentifiers()
    {
        return "Test";
    }
}
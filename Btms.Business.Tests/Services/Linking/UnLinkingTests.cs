using Btms.Backend.Data.InMemory;
using Btms.Backend.Data;
using Btms.Business.Services.Linking;
using Btms.Metrics;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.ChangeLog;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace Btms.Business.Tests.Services.Linking;

public class UnLinkingTests
{
    private static readonly Random Random = new();
    protected readonly IMongoDbContext dbContext = new MemoryMongoDbContext();
    private readonly LinkingMetrics linkingMetrics = new(new DummyMeterFactory());

    private static string GenerateDocumentReference(int id) => $"GBCVD2024.{id}";
    private static string GenerateNotificationReference(int id) => $"CHEDP.GB.2024.{id}";

    [Fact]
    public async Task Unlink_Notification_And_Movements()
    {
        // Arrange
        var testData = await AddTestData(2, 3, 1);

        var sut = CreateLinkingService();
        await sut.Link(new ImportNotificationLinkContext(testData.Cheds[0], null));
        var notificationCtx = CreateNotificationContext(testData.Cheds[0], true, false);

        // Act
        await sut.UnLink(notificationCtx);

        var loadedNotification = await dbContext.Notifications.Find(testData.Cheds[0].Id!);
        loadedNotification?.Relationships.Movements.Data.Should().BeNullOrEmpty();

        var loadedMovements = dbContext.Movements.ToList();
        loadedMovements.ForEach(m => m.Relationships.Notifications.Data.Should().BeNullOrEmpty());
    }

    protected LinkingService CreateLinkingService()
    {
        return new LinkingService(dbContext, linkingMetrics, NullLogger<LinkingService>.Instance);
    }

    protected async Task<(List<ImportNotification> Cheds, List<Movement> Movements, List<int> UnmatchedChedRefs)> AddTestData(int chedCount = 1, int movementCount = 1, int matchedChedsPerMovement = 1, int unMatchedChedsPerMovement = 0)
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
                BtmsStatus = MovementStatus.Default()
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

    protected ImportNotificationLinkContext CreateNotificationContext(ImportNotification? ched,
        bool createExistingNotification, bool fieldsOfInterest)
    {
        var chedReference = ched != null ? int.Parse(ched._MatchReference) : GenerateRandomReference();
        var etag = ched != null ? ched._Etag : string.Empty;

        return CreateNotificationContext(chedReference, etag, createExistingNotification, fieldsOfInterest);
    }

    protected ImportNotificationLinkContext CreateNotificationContext(int chedReference, string etag, bool createExistingNotification, bool fieldsOfInterest)
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

}
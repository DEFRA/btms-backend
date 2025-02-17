using Btms.Backend.Data;
using Btms.Backend.Data.InMemory;
using Btms.Business.Services.Linking;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using Btms.Types.Gvms.Mapping;
using Btms.Types.Ipaffs.Mapping;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TestDataGenerator;
using Xunit;
using ExternalReference = Btms.Types.Ipaffs.ExternalReference;
using ExternalReferenceSystemEnum = Btms.Types.Ipaffs.ExternalReferenceSystemEnum;
using ImportNotificationTypeEnum = Btms.Types.Ipaffs.ImportNotificationTypeEnum;

namespace Btms.Business.Tests.Services.Linking;

public class ImportNotificationGmrLinkerTests
{
    private MemoryMongoDbContext MongoDbContext { get; } = new();
    private ImportNotificationGmrLinker Subject { get; set; }

    public ImportNotificationGmrLinkerTests()
    {
        Subject = new ImportNotificationGmrLinker(MongoDbContext);
    }

    [Fact]
    public async Task Link_Gmr_WhenNoMrnsFromCustoms_ThenSkipped()
    {
        var mongoDbContext = Substitute.For<IMongoDbContext>();
        mongoDbContext.Notifications.Throws(new Exception("Not expected to be called"));
        Subject = new ImportNotificationGmrLinker(mongoDbContext);

        await Subject.Link(new Gmr
        {
            Declarations = new Declarations
            {
                Customs =
                [
                    new Customs { Id = "" },
                    new Customs { Id = " " }
                ]
            }
        }, CancellationToken.None);
        
        Assert.True(true, "Would have thrown if failed");
    }

    [Fact]
    public async Task Link_Gmr_WhenNoMrnsFromTransits_ThenSkipped()
    {
        var mongoDbContext = Substitute.For<IMongoDbContext>();
        mongoDbContext.Notifications.Throws(new Exception("Not expected to be called"));
        Subject = new ImportNotificationGmrLinker(mongoDbContext);

        await Subject.Link(new Gmr
        {
            Declarations = new Declarations
            {
                Transits =
                [
                    new Transits { Id = "" },
                    new Transits { Id = " " }
                ]
            }
        }, CancellationToken.None);
        
        Assert.True(true, "Would have thrown if failed");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Link_Gmr_WhenNotificationFound_ThenRelationshipsAreAdded(bool relationshipsAlreadyExist)
    {
        const string mrn = nameof(mrn);
        var notification1 = CreateImportNotification(mrn, 0);
        var notification2 = CreateImportNotification(mrn, 1);
        var referenceNumber1 = notification1.ReferenceNumber!;
        var referenceNumber2 = notification2.ReferenceNumber!;
        const string gmrId = nameof(gmrId);
        var gmr = CreateGmr(gmrId, mrn);
        if (relationshipsAlreadyExist)
        {
            notification1.Relationships.Gmrs.Data = [CreateGmrRelationship(gmrId)];
            notification2.Relationships.Gmrs.Data = [CreateGmrRelationship(gmrId)];
            gmr.Relationships.ImportNotifications.Data =
            [
                CreateImportNotificationRelationship(referenceNumber1),
                CreateImportNotificationRelationship(referenceNumber2)
            ];
        }
        await MongoDbContext.Notifications.Insert(notification1);
        await MongoDbContext.Notifications.Insert(notification2);
        await MongoDbContext.Gmrs.Insert(gmr);

        var result = await Subject.Link(gmr, CancellationToken.None);

        var notification1FromDb = await MongoDbContext.Notifications.Find(x => x.ReferenceNumber == referenceNumber1);
        var notification2FromDb = await MongoDbContext.Notifications.Find(x => x.ReferenceNumber == referenceNumber2);
        var gmrFromDb = await MongoDbContext.Gmrs.Find(x => x.Id == gmrId);
        AssertGmrRelationship(notification1FromDb, gmrId);
        AssertGmrRelationship(notification2FromDb, gmrId);
        AssertImportNotificationRelationship(gmrFromDb, referenceNumber1);
        AssertImportNotificationRelationship(gmrFromDb, referenceNumber2);
        result.From.Should().HaveCount(2);
        result.From[0].Should().BeSameAs(notification1FromDb);
        result.From[1].Should().BeSameAs(notification2FromDb);
        result.To.Should().BeSameAs(gmrFromDb);
    }

    [Fact]
    public async Task Link_ImportNotification_WhenNoMrns_ThenSkipped()
    {
        var mongoDbContext = Substitute.For<IMongoDbContext>();
        mongoDbContext.Notifications.Throws(new Exception("Not expected to be called"));
        Subject = new ImportNotificationGmrLinker(mongoDbContext);

        await Subject.Link(new ImportNotification
        {
            ExternalReferences =
            [
                new Btms.Model.Ipaffs.ExternalReference
                {
                    System = Btms.Model.Ipaffs.ExternalReferenceSystemEnum.Ncts, Reference = ""
                },
                new Btms.Model.Ipaffs.ExternalReference
                {
                    System = Btms.Model.Ipaffs.ExternalReferenceSystemEnum.Ncts, Reference = " "
                },
                new Btms.Model.Ipaffs.ExternalReference
                {
                    System = Btms.Model.Ipaffs.ExternalReferenceSystemEnum.Ecert, Reference = "123"
                }
            ]
        }, CancellationToken.None);
        
        Assert.True(true, "Would have thrown if failed");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Link_ImportNotification_WhenGmrFound_ThenRelationshipsAreAdded(bool relationshipsAlreadyExist)
    {
        const string mrn = nameof(mrn);
        const string gmrId1 = nameof(gmrId1);
        const string gmrId2 = nameof(gmrId2);
        var gmr1 = CreateGmr(gmrId1, mrn);
        var gmr2 = CreateGmr(gmrId2, mrn);
        var notification = CreateImportNotification(mrn, 0);
        var referenceNumber = notification.ReferenceNumber!;
        if (relationshipsAlreadyExist)
        {
            notification.Relationships.Gmrs.Data = [CreateGmrRelationship(gmrId1), CreateGmrRelationship(gmrId2)];
            gmr1.Relationships.ImportNotifications.Data = [CreateImportNotificationRelationship(referenceNumber)];
            gmr2.Relationships.ImportNotifications.Data = [CreateImportNotificationRelationship(referenceNumber)];
        }
        await MongoDbContext.Gmrs.Insert(gmr1);
        await MongoDbContext.Gmrs.Insert(gmr2);
        await MongoDbContext.Notifications.Insert(notification);

        var result = await Subject.Link(notification, CancellationToken.None);

        var notificationFromDb = await MongoDbContext.Notifications.Find(x => x.ReferenceNumber == referenceNumber);
        var gmr1FromDb = await MongoDbContext.Gmrs.Find(x => x.Id == gmrId1);
        var gmr2FromDb = await MongoDbContext.Gmrs.Find(x => x.Id == gmrId2);
        AssertGmrRelationship(notificationFromDb, gmrId1);
        AssertGmrRelationship(notificationFromDb, gmrId2);
        AssertImportNotificationRelationship(gmr1FromDb, referenceNumber);
        AssertImportNotificationRelationship(gmr2FromDb, referenceNumber);
        result.From.Should().HaveCount(2);
        result.From[0].Should().BeSameAs(gmr1FromDb);
        result.From[1].Should().BeSameAs(gmr2FromDb);
        result.To.Should().BeSameAs(notificationFromDb);
    }

    private static RelationshipDataItem CreateImportNotificationRelationship(string referenceNumber1)
    {
        return new RelationshipDataItem
        {
            Type = "import-notifications",
            Id = referenceNumber1,
            Links = new ResourceLink { Self = $"/api/import-notifications/{referenceNumber1}" }
        };
    }

    private static RelationshipDataItem CreateGmrRelationship(string gmrId)
    {
        return new RelationshipDataItem
        {
            Type = "gmrs",
            Id = gmrId,
            Links = new ResourceLink { Self = $"/api/gmrs/{gmrId}" }
        };
    }

    private static void AssertGmrRelationship(ImportNotification? notification, string gmrId)
    {
        notification!.Relationships.Gmrs.Data.Should().ContainEquivalentOf(new
        {
            Type = "gmrs",
            Id = gmrId,
            Links = new { Self = $"/api/gmrs/{gmrId}" }
        });
    }

    private static void AssertImportNotificationRelationship(Gmr? gmr, string referenceNumber)
    {
        gmr!.Relationships.ImportNotifications.Data.Should().ContainEquivalentOf(new
        {
            Type = "import-notifications",
            Id = referenceNumber,
            Links = new { Self = $"/api/import-notifications/{referenceNumber}" }
        });
    }

    private static ImportNotification CreateImportNotification(string mrn, int item)
    {
        return ImportNotificationBuilder.Default()
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, 1, DateTime.UtcNow, item)
            .With(x => x.ExternalReferences,
                [new ExternalReference { System = ExternalReferenceSystemEnum.Ncts, Reference = mrn }])
            .WithNoCommodities()
            .ValidateAndBuild()
            .MapWithTransform();
    }

    private static Gmr CreateGmr(string gmrId, string mrn)
    {
        return GmrBuilder.Default()
            .With(x => x.GmrId, gmrId)
            .With(x => x.Declarations,
                () => new Btms.Types.Gvms.Declarations
                {
                    Customs = [new Btms.Types.Gvms.Customs { Id = mrn }],
                    Transits = [new Btms.Types.Gvms.Transits { Id = mrn }]
                })
            .ValidateAndBuild()
            .MapWithTransform();
    }
}
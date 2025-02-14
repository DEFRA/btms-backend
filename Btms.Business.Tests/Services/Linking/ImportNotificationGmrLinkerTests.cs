using System.Linq.Expressions;
using Btms.Backend.Data;
using Btms.Backend.Data.InMemory;
using Btms.Business.Services.Linking;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using Btms.Types.Ipaffs.Mapping;
using FluentAssertions;
using NSubstitute;
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
    public async Task Link_WhenNoMrnsFromCustoms_ThenSkipped()
    {
        var mongoDbContext = Substitute.For<IMongoDbContext>();
        Subject = new ImportNotificationGmrLinker(mongoDbContext);

        await Subject.Link(new Gmr
        {
            Declarations = new Declarations
            {
                Customs =
                [
                ]
            }
        }, CancellationToken.None);

        await mongoDbContext.Notifications.DidNotReceive().Find(Arg.Any<Expression<Func<ImportNotification, bool>>>(),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Link_WhenNoMrnsFromTransits_ThenSkipped()
    {
        var mongoDbContext = Substitute.For<IMongoDbContext>();
        Subject = new ImportNotificationGmrLinker(mongoDbContext);

        await Subject.Link(new Gmr
        {
            Declarations = new Declarations
            {
                Transits = 
                [
                ]
            }
        }, CancellationToken.None);

        await mongoDbContext.Notifications.DidNotReceive().Find(Arg.Any<Expression<Func<ImportNotification, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Link_WhenNotificationFound_ThenRelationshipsAreAdded(bool relationshipsAlreadyExist)
    {
        const string mrn = nameof(mrn);
        var notification1 = CreateImportNotification(mrn, 0);
        var notification2 = CreateImportNotification(mrn, 1);
        var referenceNumber1 = notification1.ReferenceNumber!;
        var referenceNumber2 = notification2.ReferenceNumber!;
        const string gmrId = nameof(gmrId);
        var gmr = new Gmr
        {
            Id = gmrId,
            Declarations = new Declarations
            {
                Customs =
                [
                    new Customs { Id = mrn }
                ],
                Transits = 
                [
                    new Transits { Id = mrn }
                ]
            }
        };
        if (relationshipsAlreadyExist)
        {
            notification1.Relationships.Gmrs.Data = [CreateGmrRelationship(gmrId)];
            gmr.Relationships.ImportNotifications.Data = [CreateImportNotificationRelationship(referenceNumber1)];
            gmr.Relationships.ImportNotifications.Data = [CreateImportNotificationRelationship(referenceNumber2)];
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
        gmrFromDb!.Relationships.ImportNotifications.Data.Should().ContainEquivalentOf(new
        {
            Type = "import-notifications",
            Id = referenceNumber1,
            Links = new { Self = $"/api/import-notifications/{referenceNumber1}" }
        });
        result.From.Should().HaveCount(2);
        result.From[0].Should().BeSameAs(notification1FromDb);
        result.From[1].Should().BeSameAs(notification2FromDb);
        result.To.Should().BeSameAs(gmrFromDb);
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
            Type = "gmrs", Id = gmrId, Links = new { Self = $"/api/gmrs/{gmrId}" }
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
}
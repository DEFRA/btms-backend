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
    public async Task Link_WhenNoMrns_ThenSkipped()
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Link_WhenNotificationFound_ThenRelationshipsAreAdded(bool relationshipsAlreadyExist)
    {
        const string mrn = nameof(mrn);
        const string referenceNumber = nameof(ImportNotification.ReferenceNumber);
        var notification = ImportNotificationBuilder.Default()
            .With(x => x.ReferenceNumber, referenceNumber)
            .With(x => x.ExternalReferences,
                [new ExternalReference { System = ExternalReferenceSystemEnum.Ncts, Reference = mrn }])
            .WithNoCommodities()
            .ValidateAndBuild()
            .MapWithTransform();
        const string gmrId = nameof(gmrId);
        var gmr = new Gmr
        {
            Id = gmrId,
            Declarations = new Declarations
            {
                Customs =
                [
                    new Customs { Id = mrn }
                ]
            }
        };
        if (relationshipsAlreadyExist)
        {
            notification.Relationships.Gmrs.Data = [new RelationshipDataItem
            {
                Type = "gmrs",
                Id = gmrId,
                Links = new ResourceLink { Self = $"/api/gmrs/{gmrId}" }
            }];
            gmr.Relationships.ImportNotifications.Data = [new RelationshipDataItem
            {
                Type = "import-notifications",
                Id = referenceNumber,
                Links = new ResourceLink { Self = $"/api/import-notifications/{referenceNumber}" }
            }];
        }
        await MongoDbContext.Notifications.Insert(notification);
        await MongoDbContext.Gmrs.Insert(gmr);

        await Subject.Link(gmr, CancellationToken.None);

        (await MongoDbContext.Notifications.Find(x => x.ReferenceNumber == referenceNumber))!.Relationships.Gmrs.Data
            .Should().ContainEquivalentOf(
                new { Type = "gmrs", Id = gmrId, Links = new { Self = $"/api/gmrs/{gmrId}" } });
        (await MongoDbContext.Gmrs.Find(x => x.Id == gmrId))!.Relationships.ImportNotifications.Data.Should()
            .ContainEquivalentOf(new
            {
                Type = "import-notifications",
                Id = referenceNumber,
                Links = new { Self = $"/api/import-notifications/{referenceNumber}" }
            });
    }
}
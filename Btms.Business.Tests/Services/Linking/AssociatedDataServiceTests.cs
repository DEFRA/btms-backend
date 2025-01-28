using Btms.Backend.Data.InMemory;
using Btms.Business.Services.Linking;
using Btms.Business.Tests.Helpers;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Linking;

public class AssociatedDataServiceTests
{
    private string _existingId = nameof(_existingId);
    private readonly DateTime _existingUpdated = new(2025, 1, 28, 11, 0, 0, DateTimeKind.Utc);
    private readonly DateTimeOffset _timeProviderNow = new(2025, 1, 28, 12, 0, 0, TimeSpan.Zero);
    
    private readonly MemoryMongoDbContext _mongoDbContext = new();

    [Fact]
    public async Task UpdateRelationships_ExistingNotification_NoRelationships_DoesNotUpdate()
    {
        var subject = await CreateSubject();
        var existingNotification = await _mongoDbContext.Notifications.Find(x => x.Id == _existingId) ??
                                   throw new InvalidOperationException();
        var etag = existingNotification._Etag;
        
        await subject.UpdateRelationships([existingNotification], new Movement
        {
            BtmsStatus = MovementStatus.Default()
        }, CancellationToken.None);

        existingNotification._Etag.Should().Be(etag);
    }

    [Fact]
    public async Task UpdateRelationships_ExistingNotification_NoMatchingRelationships_DoesNotUpdate()
    {
        var movement = new Movement
        {
            Id = "123456789",
            Updated = _existingUpdated,
            BtmsStatus = MovementStatus.Default()
        };
        var subject = await CreateSubject(movement: movement);
        var existingNotification = await _mongoDbContext.Notifications.Find(x => x.Id == _existingId) ??
                                   throw new InvalidOperationException();
        var etag = existingNotification._Etag;
        
        await subject.UpdateRelationships([existingNotification], new Movement
        {
            Id = "different-id",
            BtmsStatus = MovementStatus.Default()
        }, CancellationToken.None);

        existingNotification._Etag.Should().Be(etag);
        existingNotification.Relationships.Movements.Data.Should().ContainSingle();
        existingNotification.Relationships.Movements.Data[0].Updated.Should().Be(_existingUpdated);
    }

    [Theory]
    [InlineData("123456789", "123456789")]
    [InlineData("mrn", "MRN")]
    public async Task UpdateRelationships_ExistingNotification_MatchingRelationships_ShouldUpdate(string existingId, string matchingId)
    {
        var movement = new Movement
        {
            Id = existingId,
            Updated = _existingUpdated,
            BtmsStatus = MovementStatus.Default()
        };
        var subject = await CreateSubject(movement: movement);
        var existingNotification = await _mongoDbContext.Notifications.Find(x => x.Id == _existingId) ??
                                   throw new InvalidOperationException();
        var etag = existingNotification._Etag;
        
        await subject.UpdateRelationships([existingNotification], new Movement
        {
            Id = matchingId,
            BtmsStatus = MovementStatus.Default()
        }, CancellationToken.None);

        existingNotification._Etag.Should().NotBe(etag);
        existingNotification.Relationships.Movements.Data.Should().ContainSingle();
        existingNotification.Relationships.Movements.Data[0].Updated.Should().Be(_timeProviderNow.UtcDateTime);
    }

    private async Task<AssociatedDataService> CreateSubject(Movement? movement = null)
    {
        var notification = new ImportNotification { Id = _existingId };

        if (movement is not null)
            notification.AddRelationship(new TdmRelationshipObject
            {
                Data =
                [
                    new RelationshipDataItem { Type = "movements", Id = movement.Id!, Updated = _existingUpdated }
                ]
            });
        
        await _mongoDbContext.Notifications.Insert(notification);

        return new AssociatedDataService(_mongoDbContext, new FrozenTimeProvider(_timeProviderNow));
    }
}
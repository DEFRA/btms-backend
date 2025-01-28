using Btms.Backend.Data;
using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public class AssociatedDataService(IMongoDbContext mongoDbContext, TimeProvider timeProvider) : IAssociatedDataService
{
    public async Task UpdateRelationships(List<ImportNotification> notifications, Movement movement, CancellationToken cancellationToken)
    {
        foreach (var notification in notifications)
        {
            var changed = false;

            foreach (var relationship in notification.Relationships.Movements.Data.Where(x =>
                         string.Equals(x.Id, movement.Id, StringComparison.OrdinalIgnoreCase)))
            {
                relationship.Updated = timeProvider.GetUtcNow().UtcDateTime;
                changed = true;
            }

            if (changed)
            {
                // Assumes the list of notifications exists in the DB already
                await mongoDbContext.Notifications.Update(
                    notification, 
                    notification._Etag, 
                    setUpdated: false,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
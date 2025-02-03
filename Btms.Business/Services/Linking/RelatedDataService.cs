using Btms.Backend.Data;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public class RelatedDataService(IMongoDbContext mongoDbContext) : IRelatedDataService
{
    /// <inheritdoc />
    public async Task RelatedDataEntityChanged(
        List<ImportNotification> notifications, 
        CancellationToken cancellationToken)
    {
        foreach (var notification in notifications)
        {
            // Assumes the list of notifications exists in the DB already
            await mongoDbContext.Notifications.Update(notification, cancellationToken: cancellationToken);
        }
    }
}
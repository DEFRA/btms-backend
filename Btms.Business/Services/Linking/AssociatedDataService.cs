using Btms.Backend.Data;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public class AssociatedDataService(IMongoDbContext mongoDbContext) : IAssociatedDataService
{
    /// <inheritdoc />
    public async Task RelatedDataEntityChanged(
        List<ImportNotification> notifications, 
        string auditId, 
        CancellationToken cancellationToken)
    {
        foreach (var notification in notifications)
        {
            notification.RelatedDataChanged(auditId);
            
            // Assumes the list of notifications exists in the DB already
            await mongoDbContext.Notifications.Update(
                notification, 
                notification._Etag, 
                cancellationToken: cancellationToken);
        }
    }
}
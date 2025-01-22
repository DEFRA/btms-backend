using Btms.Backend.Data;
using Btms.Model.Auditing;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public class AssociatedDataService(IMongoDbContext mongoDbContext) : IAssociatedDataService
{
    public async Task Update(
        List<ImportNotification> notifications, 
        string auditEntryId, 
        CancellationToken cancellationToken)
    {
        foreach (var notification in notifications)
        {
            var auditEntry = AuditEntry.CreateAssociatedUpdate(
                auditEntryId,
                notification.AuditEntries.LastOrDefault()?.Version + 1 ?? 1);
            
            notification.Changed(auditEntry);
            
            // Assumes the list of notifications exists in the DB already
            await mongoDbContext.Notifications.Update(
                notification, 
                notification._Etag, 
                cancellationToken: cancellationToken);
        }
    }
}
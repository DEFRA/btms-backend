using Cdms.Backend.Data;
using Cdms.Types.Ipaffs;
using Cdms.Types.Ipaffs.Mapping;
using SlimMessageBus;

namespace Cdms.Consumers
{
    internal class NotificationConsumer(IMongoDbContext dbContext)
        : IConsumer<ImportNotification>, IConsumerWithContext
    {
        public async Task OnHandle(ImportNotification message)
        {
            var internalNotification = message.MapWithTransform();
            var auditId = Context.Headers["messageId"].ToString();

            var existingNotification = await dbContext.Notifications.Find(message.ReferenceNumber);
            if (existingNotification is not null)
            {
                if (internalNotification.LastUpdated > existingNotification.LastUpdated)
                {
                    internalNotification.AuditEntries = existingNotification.AuditEntries;
                    internalNotification.Updated(BuildNormalizedIpaffsPath(auditId!), existingNotification);
                    await dbContext.Notifications.Update(internalNotification, existingNotification._Etag);
                }
                else
                {
                    //TODO: when an older notification is processed what should happen here?
                }
            }
            else
            {
                internalNotification.Created(BuildNormalizedIpaffsPath(auditId));
                await dbContext.Notifications.Insert(internalNotification);
            }
        }

        public IConsumerContext Context { get; set; }

        private string BuildNormalizedIpaffsPath(string fullPath)
        {
            return fullPath.Replace("RAW/IPAFFS/", "");
        }
    }
}
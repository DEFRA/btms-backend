using Cdms.Backend.Data;
using Cdms.Business.Services;
using Cdms.Common.Extensions;
using Cdms.Types.Ipaffs;
using Cdms.Types.Ipaffs.Mapping;
using SlimMessageBus;
using System.Diagnostics.CodeAnalysis;

namespace Cdms.Consumers
{
    internal class NotificationConsumer(IMongoDbContext dbContext, ILinkingService linkingService)
        : IConsumer<ImportNotification>, IConsumerWithContext
    {
        private ILinkingService linkingService { get; } = linkingService;

        [SuppressMessage("SonarLint", "S1481",
            Justification =
                "LinkResult variable is unused until matching and decisions are implemented")]
        public async Task OnHandle(ImportNotification message)
        {
            var internalNotification = message.MapWithTransform();
            var auditId = Context.Headers["messageId"].ToString();

            var existingNotification = await dbContext.Notifications.Find(message.ReferenceNumber!);
            if (existingNotification is not null)
            {
                if (internalNotification.UpdatedSource.TrimMicroseconds() > existingNotification.UpdatedSource.TrimMicroseconds())
                {
                    internalNotification.AuditEntries = existingNotification.AuditEntries;
                    internalNotification.Update(BuildNormalizedIpaffsPath(auditId!), existingNotification);
                    await dbContext.Notifications.Update(internalNotification, existingNotification._Etag);
                }
                else
                {
                    //when an older notification is processed what should happen here?
                }
            }
            else
            {
                internalNotification.Create(BuildNormalizedIpaffsPath(auditId!));
                await dbContext.Notifications.Insert(internalNotification);
            }

            var linkContext = new ImportNotificationLinkContext(internalNotification, existingNotification);
            var linkResult = await linkingService.Link(linkContext, Context.CancellationToken);
        }

        public IConsumerContext Context { get; set; } = null!;

        private static string BuildNormalizedIpaffsPath(string fullPath)
        {
            return fullPath.Replace("RAW/IPAFFS/", "");
        }
    }
}
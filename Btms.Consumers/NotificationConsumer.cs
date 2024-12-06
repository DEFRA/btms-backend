using Btms.Backend.Data;
using Btms.Business.Services;
using Btms.Common.Extensions;
using Btms.Types.Ipaffs;
using Btms.Types.Ipaffs.Mapping;
using SlimMessageBus;
using System.Diagnostics.CodeAnalysis;
using Btms.Consumers.Extensions;
using Force.DeepCloner;
using Microsoft.Extensions.Logging;

namespace Btms.Consumers
{
    internal class NotificationConsumer(IMongoDbContext dbContext, ILinkingService linkingService, ILogger<NotificationConsumer> logger)
        : IConsumer<ImportNotification>, IConsumerWithContext
    {
        private ILinkingService linkingService { get; } = linkingService;

        [SuppressMessage("SonarLint", "S1481",
            Justification =
                "LinkResult variable is unused until matching and decisions are implemented")]
        public async Task OnHandle(ImportNotification message)
        {
            var auditId = Context.Headers["messageId"].ToString();
            logger.ConsumerStarted(Context.GetJobId()!, auditId!, GetType().Name, message.ReferenceNumber!);
            using (logger.BeginScope(new List<KeyValuePair<string, object>>
                   {
                       new("JobId", Context.GetJobId()!),
                       new("MessageId", auditId!),
                       new("Consumer", GetType().Name),
                       new("Identifier", message.ReferenceNumber!),
                   }))
            {
                var internalNotification = message.MapWithTransform();
                

                var existingNotification = await dbContext.Notifications.Find(message.ReferenceNumber!);
                Model.Ipaffs.ImportNotification persistedNotification = null!;
                if (existingNotification is not null)
                {
                    persistedNotification = existingNotification.DeepClone();
                    if (internalNotification.UpdatedSource.TrimMicroseconds() >
                        existingNotification.UpdatedSource.TrimMicroseconds())
                    {
                        internalNotification.AuditEntries = existingNotification.AuditEntries;
                        internalNotification.CreatedSource = existingNotification.CreatedSource;
                        internalNotification.Update(BuildNormalizedIpaffsPath(auditId!), existingNotification);
                        await dbContext.Notifications.Update(internalNotification, existingNotification._Etag);
                    }
                    else
                    {
                        logger.MessageSkipped(Context.GetJobId()!, auditId!, GetType().Name, message.ReferenceNumber!);
                        Context.Skipped();
                    }
                }
                else
                {
                    internalNotification.Create(BuildNormalizedIpaffsPath(auditId!));
                    await dbContext.Notifications.Insert(internalNotification);
                    persistedNotification = internalNotification!;
                }

                var linkContext = new ImportNotificationLinkContext(persistedNotification, existingNotification);
                var linkResult = await linkingService.Link(linkContext, Context.CancellationToken);
            }
        }

        public IConsumerContext Context { get; set; } = null!;

        private static string BuildNormalizedIpaffsPath(string fullPath)
        {
            return fullPath.Replace("RAW/IPAFFS/", "");
        }
    }
}
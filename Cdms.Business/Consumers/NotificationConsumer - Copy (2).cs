//using Cdms.Backend.Data;
//using Cdms.Model.Auditing;
//using Cdms.Types.Ipaffs;
//using Cdms.Types.Ipaffs.Mapping;
//using SlimMessageBus;
//using SlimMessageBus.Host.Interceptor;
//using System.Diagnostics;
//using System.Diagnostics.Metrics;

//namespace Cdms.Business.Consumers
//{
//    public class ImportNotificationMetrics
//    {
//        private readonly Counter<int> processed;

//        public ImportNotificationMetrics(IMeterFactory meterFactory)
//        {
//            var meter = meterFactory.Create("Cdms");
//            processed = meter.CreateCounter<int>("cdms.importnotifications.processed");
//        }

//        public void MessageProcessed(ImportNotificationTypeEnum notificationType)
//        {
//            processed.Add(1,
//                new KeyValuePair<string, object?>("importnotification.type", notificationType.ToString()));
//        }
//    }


//    internal class NotificationConsumer(IMongoDbContext dbContext, ImportNotificationMetrics metrics)
//        : IConsumer<ImportNotification>, IConsumerWithContext
//    {
//        public async Task OnHandle(ImportNotification message)
//        {
//            var internalNotification = message.MapWithTransform();
//            var auditId = Context.Headers["messageId"].ToString();
//            AuditEntry auditEntry = null;


//            var existingNotification = await dbContext.Notifications.Find(message.ReferenceNumber);
//            if (existingNotification is not null)
//            {
//                if (internalNotification.Version > existingNotification.Version)
//                {
//                    internalNotification.AuditEntries = existingNotification.AuditEntries;

//                    if ((message.Version - existingNotification.Version) == 1)
//                    {
//                        auditEntry = AuditEntry.CreateUpdated(existingNotification,
//                            internalNotification,
//                            BuildNormalizedIpaffsPath(auditId),
//                            internalNotification.Version.GetValueOrDefault(),
//                            internalNotification.LastUpdated,
//                            internalNotification.LastUpdatedBy?.DisplayName);
//                    }
//                    else
//                    {
//                        auditEntry = AuditEntry.CreateSkippedVersion(
//                            internalNotification,
//                            BuildNormalizedIpaffsPath(auditId),
//                            message.Version.GetValueOrDefault(),
//                            internalNotification.LastUpdated,
//                            message.LastUpdatedBy?.DisplayName);
//                    }

//                    internalNotification.Update(auditEntry);
//                    await dbContext.Notifications.Update(internalNotification);
//                }
//            }
//            else
//            {
//                auditEntry = AuditEntry.CreateCreatedEntry(
//                    internalNotification,
//                    BuildNormalizedIpaffsPath(auditId),
//                    internalNotification.Version.GetValueOrDefault(),
//                    internalNotification.LastUpdated,
//                    internalNotification.LastUpdatedBy?.DisplayName);
//                internalNotification.Update(auditEntry);
//                await dbContext.Notifications.Insert(internalNotification);
//            }

//            metrics.MessageProcessed(message.ImportNotificationType.Value);
//        }

//        public IConsumerContext Context { get; set; }

//        private string BuildNormalizedIpaffsPath(string fullPath)
//        {
//            return fullPath.Replace("RAW/IPAFFS/", "");
//        }
//    }
//}


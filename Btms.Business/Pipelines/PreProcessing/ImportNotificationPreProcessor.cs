using Btms.Backend.Data;
using Btms.Common.Extensions;
using Btms.Model.ChangeLog;
using Btms.Types.Ipaffs;
using Btms.Types.Ipaffs.Mapping;
using Microsoft.Extensions.Logging;
using ImportNotificationStatusEnum = Btms.Model.Ipaffs.ImportNotificationStatusEnum;

namespace Btms.Business.Pipelines.PreProcessing;

public class ImportNotificationPreProcessor(IMongoDbContext dbContext, ILogger<ImportNotificationPreProcessor> logger) : IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification>
{
    public async Task<PreProcessingResult<Model.Ipaffs.ImportNotification>> Process(PreProcessingContext<ImportNotification> preProcessingContext)
    {
        if (preProcessingContext.Message.Status == Types.Ipaffs.ImportNotificationStatusEnum.Amend
            || preProcessingContext.Message.Status == Types.Ipaffs.ImportNotificationStatusEnum.Draft)
        {
            return PreProcessResult.NotProcessed<Model.Ipaffs.ImportNotification>();
        }


        var internalNotification = preProcessingContext.Message.MapWithTransform();
        var existingNotification = await dbContext.Notifications.Find(preProcessingContext.Message.ReferenceNumber!);

        if (existingNotification is null)
        {
            internalNotification.Create(preProcessingContext.MessageId);
            await dbContext.Notifications.Insert(internalNotification);
            return PreProcessResult.New(internalNotification);
        }

        
        if (internalNotification.UpdatedSource.TrimMicroseconds() >
            existingNotification.UpdatedSource.TrimMicroseconds())
        {
            internalNotification.AuditEntries = existingNotification.AuditEntries;
            internalNotification.CreatedSource = existingNotification.CreatedSource;
            internalNotification.Relationships = existingNotification.Relationships;
            internalNotification._Etag = existingNotification._Etag;

            var changeSet = internalNotification.GenerateChangeSet(existingNotification);

            switch (internalNotification.Status)
            {
                case ImportNotificationStatusEnum.Cancelled:
                    internalNotification.Cancel(preProcessingContext.MessageId, changeSet);
                    break;
                case ImportNotificationStatusEnum.Deleted:
                    internalNotification.Delete(preProcessingContext.MessageId, changeSet);
                    break;
                default:
                    internalNotification.Update(preProcessingContext.MessageId, changeSet);
                    break;
            }

            
            await dbContext.Notifications.Update(internalNotification, existingNotification._Etag);

            return PreProcessResult.Changed(internalNotification, changeSet);
        }
        
        if (internalNotification.UpdatedSource.TrimMicroseconds() ==
                 existingNotification.UpdatedSource.TrimMicroseconds())
        {
            return PreProcessResult.AlreadyProcessed(existingNotification);
        }
        
        logger.MessageSkipped(preProcessingContext.MessageId, preProcessingContext.Message.ReferenceNumber!);
        return PreProcessResult.Skipped(existingNotification);
        
    }
}
using Btms.Backend.Data;
using Btms.Common.Extensions;
using Btms.Model.ChangeLog;
using Btms.Types.Ipaffs;
using Btms.Types.Ipaffs.Mapping;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Pipelines.PreProcessing;

public class ImportNotificationPreProcessor(IMongoDbContext dbContext, ILogger<ImportNotificationPreProcessor> logger) : IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification>
{
    public async Task<PreProcessingResult<Model.Ipaffs.ImportNotification>> Process(PreProcessingContext<ImportNotification> preProcessingContext)
    {
        var internalNotification = preProcessingContext.Message.MapWithTransform();
        ArgumentNullException.ThrowIfNull(internalNotification, nameof(internalNotification));
        var existingNotification = await dbContext.Notifications.Find(preProcessingContext.Message.ReferenceNumber!);

        if (existingNotification is null)
        {
            internalNotification.Create(preProcessingContext.MessageId);
            await dbContext.Notifications.Insert(internalNotification);
            return PreProcessResult.New(internalNotification);
        }
        
        if (internalNotification.LastUpdated.TrimMicroseconds() > existingNotification.LastUpdated.TrimMicroseconds())
        {
            internalNotification.AuditEntries = existingNotification.AuditEntries;
            internalNotification.CreatedSource = existingNotification.CreatedSource;

            var changeSet = internalNotification.GenerateChangeSet(existingNotification);

            internalNotification.Update(preProcessingContext.MessageId, changeSet);
            await dbContext.Notifications.Update(internalNotification, existingNotification._Etag);

            return PreProcessResult.Changed(internalNotification, changeSet);
        }
        
        if (internalNotification.LastUpdated.TrimMicroseconds() == existingNotification.LastUpdated.TrimMicroseconds())
        {
            return PreProcessResult.AlreadyProcessed(existingNotification);
        }
        
        logger.MessageSkipped(preProcessingContext.MessageId, preProcessingContext.Message.ReferenceNumber!);
        return PreProcessResult.Skipped(existingNotification);
    }
}
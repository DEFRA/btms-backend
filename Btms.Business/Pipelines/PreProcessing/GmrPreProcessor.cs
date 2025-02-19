using Btms.Backend.Data;
using Btms.Model.Auditing;
using Btms.Model.ChangeLog;
using Btms.Types.Gvms;
using Btms.Types.Gvms.Mapping;

namespace Btms.Business.Pipelines.PreProcessing;

public class GmrPreProcessor(IMongoDbContext mongoDbContext) : IPreProcessor<Gmr, Model.Gvms.Gmr>
{
    public async Task<PreProcessingResult<Model.Gvms.Gmr>> Process(PreProcessingContext<Gmr> preProcessingContext,
        CancellationToken cancellationToken = default)
    {
        var message = preProcessingContext.Message;

        // Pre-processing needs to be able to skip if input data is incorrect, however the current
        // pattern mandates a record, which in turn mandates an audit entry
        if (message.GmrId is null)
            return PreProcessResult.Skipped(new Model.Gvms.Gmr { AuditEntries = [new AuditEntry()] });

        var existingGmr = await mongoDbContext.Gmrs.Find(message.GmrId, cancellationToken);

        if (existingGmr is null)
            return await Insert(preProcessingContext, cancellationToken);

        if (message.UpdatedSource > existingGmr.UpdatedSource)
            return await Update(preProcessingContext, existingGmr, cancellationToken);

        return PreProcessResult.Skipped(existingGmr);
    }

    private async Task<PreProcessingResult<Model.Gvms.Gmr>> Insert(
        PreProcessingContext<Gmr> preProcessingContext,
        CancellationToken cancellationToken)
    {
        var mappedGmr = preProcessingContext.Message.MapWithTransform();
        var auditId = preProcessingContext.MessageId;

        var auditEntry = AuditEntry.CreateCreatedEntry(
            mappedGmr,
            auditId,
            1,
            preProcessingContext.Message.UpdatedSource,
            CreatedBySystem.Gvms);

        mappedGmr.Changed(auditEntry);

        await mongoDbContext.Gmrs.Insert(mappedGmr, cancellationToken);

        return PreProcessResult.New(mappedGmr);
    }

    private async Task<PreProcessingResult<Model.Gvms.Gmr>> Update(
        PreProcessingContext<Gmr> preProcessingContext,
        Model.Gvms.Gmr existingGmr,
        CancellationToken cancellationToken)
    {
        var mappedGmr = preProcessingContext.Message.MapWithTransform();
        var auditId = preProcessingContext.MessageId;

        mappedGmr.Created = existingGmr.Created;
        mappedGmr.AuditEntries = existingGmr.AuditEntries;
        mappedGmr.Relationships = existingGmr.Relationships;
        mappedGmr._Etag = existingGmr._Etag;

        var auditEntry = AuditEntry.CreateUpdated(
            existingGmr,
            mappedGmr,
            auditId,
            mappedGmr.AuditEntries.Count + 1,
            preProcessingContext.Message.UpdatedSource,
            CreatedBySystem.Gvms);

        mappedGmr.Changed(auditEntry);

        await mongoDbContext.Gmrs.Update(mappedGmr, cancellationToken);

        return PreProcessResult.Changed(mappedGmr, mappedGmr.GenerateChangeSet(existingGmr));
    }
}
using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Model.ChangeLog;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Pipelines.PreProcessing;

public class MovementPreProcessor(IMongoDbContext dbContext, ILogger<MovementPreProcessor> logger, MovementBuilder movementBuilder) : IPreProcessor<AlvsClearanceRequest, Movement>
{
    public async Task<PreProcessingResult<Movement>> Process(PreProcessingContext<AlvsClearanceRequest> preProcessingContext)
    {

        var internalClearanceRequest = AlvsClearanceRequestMapper.Map(preProcessingContext.Message);
        var mb = movementBuilder.From(internalClearanceRequest);
        var existingMovement = await dbContext.Movements.Find(mb.Id);

        if (existingMovement is null)
        {
            // ArgumentNullException.ThrowIfNull(movement);
            
            var auditEntry = mb.CreateAuditEntry(
                preProcessingContext.MessageId,
                AuditEntry.CreatedByCds
            );
            
            mb.Update(auditEntry);
            var movement = mb.Build();
            await dbContext.Movements.Insert(movement);
            return PreProcessResult.New(movement);
        }

        // if (movement.ClearanceRequests[^1].Header?.EntryVersionNumber > existingMovement.ClearanceRequests[0].Header?.EntryVersionNumber)
        if (mb.IsEntryVersionNumberGreaterThan(existingMovement.ClearanceRequests[0].Header?.EntryVersionNumber))
        {
            var existingBuilder = movementBuilder.From(existingMovement);
            // var changeSet = movement.ClearanceRequests[^1].GenerateChangeSet(existingMovement.ClearanceRequests[0]);
            var changeSet = existingBuilder.GenerateChangeSet(mb);
            
            var auditEntry = mb.UpdateAuditEntry(
                preProcessingContext.MessageId,
                AuditEntry.CreatedByCds,
                changeSet
            );

            mb.Update(auditEntry);

            existingBuilder.ReplaceClearanceRequests(mb);
            
            // existingMovement.ClearanceRequests.RemoveAll(x =>
            //     x.Header?.EntryReference ==
            //     movement.ClearanceRequests[0].Header?.EntryReference);
            // existingMovement.ClearanceRequests.AddRange(movement.ClearanceRequests);
            //
            // existingMovement.Items.AddRange(movement.Items);
            
            await dbContext.Movements.Update(existingMovement, existingMovement._Etag);
            return PreProcessResult.Changed(existingMovement, changeSet);
        }

        if (mb.IsEntryVersionNumberEqualTo(existingMovement.ClearanceRequests[0].Header?.EntryVersionNumber))
        {
            return PreProcessResult.AlreadyProcessed(existingMovement);
        }

        logger.MessageSkipped(preProcessingContext.MessageId, preProcessingContext.Message.Header?.EntryReference!);
        return PreProcessResult.Skipped(existingMovement);
        
    }
}
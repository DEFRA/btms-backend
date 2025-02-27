using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Pipelines.PreProcessing;

public class MovementPreProcessor(IMongoDbContext dbContext, ILogger<MovementPreProcessor> logger, MovementBuilderFactory movementBuilderFactory) : IPreProcessor<AlvsClearanceRequest, Movement>
{
    public async Task<PreProcessingResult<Movement>> Process(
        PreProcessingContext<AlvsClearanceRequest> preProcessingContext, CancellationToken cancellationToken = default)
    {
        var internalClearanceRequest = AlvsClearanceRequestMapper.Map(preProcessingContext.Message);
        var mb = movementBuilderFactory.From(internalClearanceRequest);
        var existingMovement = await dbContext.Movements.Find(mb.Id);

        if (existingMovement is null)
        {
            var auditEntry = mb.CreateAuditEntry(
                preProcessingContext.MessageId,
                CreatedBySystem.Cds
            );

            mb.Update(auditEntry);
            var movement = mb.Build();
            await dbContext.Movements.Insert(movement);
            return PreProcessResult.New(movement);
        }

        var existingBuilder = movementBuilderFactory.From(existingMovement);

        if (mb.IsEntryVersionNumberGreaterThan(existingMovement.ClearanceRequests[0].Header?.EntryVersionNumber))
        {
            var changeSet = mb.GenerateChangeSet(existingBuilder);

            var auditEntry = mb.UpdateAuditEntry(
                preProcessingContext.MessageId,
                CreatedBySystem.Cds,
                changeSet
            );

            existingBuilder.Update(auditEntry);

            existingBuilder.ReplaceClearanceRequests(mb);

            await dbContext.Movements.Update(existingMovement);

            return PreProcessResult.Changed(existingMovement, changeSet);
        }

        PreProcessingResult<Movement> result;

        if (mb.IsEntryVersionNumberEqualTo(existingMovement.ClearanceRequests[0].Header?.EntryVersionNumber))
        {
            result = PreProcessResult.AlreadyProcessed(existingMovement);
        }
        else
        {
            logger.MessageSkipped(preProcessingContext.MessageId, preProcessingContext.Message.Header?.EntryReference!);
            result = PreProcessResult.Skipped(existingMovement);
        }

        var skippedAuditEntry = existingBuilder.SkippedAuditEntry(
            preProcessingContext.MessageId,
            CreatedBySystem.Cds);

        existingBuilder.Update(skippedAuditEntry);

        return result;

    }
}
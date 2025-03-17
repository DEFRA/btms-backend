using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Model.Cds;
using Btms.Model.Validation;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using Btms.Validation;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Btms.Business.Pipelines.PreProcessing;

public class MovementPreProcessor(IMongoDbContext dbContext, ILogger<MovementPreProcessor> logger, MovementBuilderFactory movementBuilderFactory, IBtmsValidator validator) : IPreProcessor<AlvsClearanceRequest, Movement>
{
    public async Task<PreProcessingResult<Movement>> Process(
        PreProcessingContext<AlvsClearanceRequest> preProcessingContext, CancellationToken cancellationToken = default)
    {
        var internalClearanceRequest = AlvsClearanceRequestMapper.Map(preProcessingContext.Message);
        var mb = movementBuilderFactory.From(internalClearanceRequest);
        var existingMovement = await dbContext.Movements.Find(mb.Id);

        if (!await Validate(preProcessingContext.MessageId, preProcessingContext.Message, internalClearanceRequest, existingMovement, cancellationToken))
        {
            return PreProcessResult.ValidationError<Movement>();
        }

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

    private async Task<bool> Validate(string auditId, AlvsClearanceRequest message, CdsClearanceRequest clearanceRequest, Movement? existing, CancellationToken cancellationToken)
    {
        var schemaValidationResult = validator.Validate(message);
        var modelValidationResult = validator.Validate(new BtmsValidationPair<CdsClearanceRequest, Movement>(clearanceRequest, existing), "CdsClearanceRequest_Movement");

        schemaValidationResult.Merge(modelValidationResult);

        if (!schemaValidationResult.IsValid)
        {
            await dbContext.CdsValidationErrors.Insert(
                new CdsValidationError()
                {
                    Id = $"{nameof(AlvsClearanceRequest)}_{auditId}",
                    Type = nameof(AlvsClearanceRequest),
                    Data = BsonDocument.Parse(GeneralExtensions.ToJson(message)),
                    ValidationResult = schemaValidationResult
                });
            await dbContext.SaveChangesAsync(cancellation: cancellationToken);
            return false;
        }

        return true;
    }
}
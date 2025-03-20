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
        if (!await SchemaValidate(preProcessingContext.MessageId, preProcessingContext.Message, cancellationToken))
        {
            return PreProcessResult.ValidationError<Movement>();
        }

        var internalClearanceRequest = AlvsClearanceRequestMapper.Map(preProcessingContext.Message);
        var mb = movementBuilderFactory.From(internalClearanceRequest);
        var existingMovement = await dbContext.Movements.Find(mb.Id);

        if (!await ModelValidate(preProcessingContext.MessageId, internalClearanceRequest, existingMovement, cancellationToken))
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

    private async Task<bool> ModelValidate(string auditId, CdsClearanceRequest clearanceRequest, Movement? existing, CancellationToken cancellationToken)
    {
        var model = new BtmsValidationPair<CdsClearanceRequest, Movement>(clearanceRequest, existing);
        var modelValidationResult = validator.Validate(model, "CdsClearanceRequest_Movement");

        if (!modelValidationResult.IsValid)
        {
            await dbContext.CdsValidationErrors.Insert(
                new CdsValidationError()
                {
                    Id = $"{nameof(AlvsClearanceRequest)}_{auditId}",
                    Type = nameof(AlvsClearanceRequest),
                    Data = BsonDocument.Parse(GeneralExtensions.ToJson(model)),
                    ValidationResult = modelValidationResult
                });
            await dbContext.SaveChangesAsync(cancellation: cancellationToken);
            return false;
        }

        return true;
    }

    private async Task<bool> SchemaValidate(string auditId, AlvsClearanceRequest message, CancellationToken cancellationToken)
    {
        var schemaValidationResult = validator.Validate(message);

        if (!schemaValidationResult.IsValid)
        {
            await dbContext.CdsValidationErrors.Insert(
                new CdsValidationError()
                {
                    Id = $"{nameof(AlvsClearanceRequest)}_{auditId}",
                    Type = nameof(AlvsClearanceRequest),
                    Data = BsonDocument.Parse(GeneralExtensions.ToJson(message)),
                    ValidationResult = schemaValidationResult
                }, cancellationToken);
            await dbContext.SaveChangesAsync(cancellation: cancellationToken);
            return false;
        }

        return true;
    }
}
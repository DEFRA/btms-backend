using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Common.Extensions;
using Btms.Consumers.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Validation;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using Btms.Validation;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SlimMessageBus;
using System.Threading;
using Finalisation = Btms.Types.Alvs.Finalisation;

namespace Btms.Consumers;

public class FinalisationsConsumer(IMongoDbContext dbContext,
    MovementBuilderFactory movementBuilderFactory,
    ILogger<FinalisationsConsumer> logger,
    IBtmsValidator validator)
    : IConsumer<Finalisation>, IConsumerWithContext
{
    public async Task OnHandle(Finalisation message, CancellationToken cancellationToken)
    {
        var auditId = Context.GetMessageId();
        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);
        var internalFinalisation = FinalisationMapper.Map(message);

        if (!await Validate(auditId, message, internalFinalisation, existingMovement))
        {
            return;
        }

        if (existingMovement != null)
        {
            logger.LogInformation("Finalisation received");

            var existingMovementBuilder = movementBuilderFactory
                .From(existingMovement!)
                .MergeFinalisation(auditId!, internalFinalisation);

            if (existingMovementBuilder.HasChanges)
            {
                await dbContext.Movements.Update(existingMovementBuilder.Build(), existingMovement._Etag, cancellationToken);
                await dbContext.SaveChangesAsync(cancellation: Context.CancellationToken);
            }
        }
    }

    private async Task<bool> Validate(string auditId, Finalisation message, CdsFinalisation finalisation, Movement? existing)
    {
        var schemaValidationResult = validator.Validate(message);
        var modelValidationResult = validator.Validate(new BtmsValidationPair<CdsFinalisation, Movement>(finalisation, existing), "CdsFinalisation_Movement");

        schemaValidationResult.Merge(modelValidationResult);

        if (!schemaValidationResult.IsValid)
        {
            await dbContext.CdsValidationErrors.Insert(
                new CdsValidationError()
                {
                    Id = $"{nameof(Finalisation)}_{auditId}",
                    Type = nameof(Finalisation),
                    Data = BsonDocument.Parse(GeneralExtensions.ToJson(message)),
                    ValidationResult = schemaValidationResult
                });
            await dbContext.SaveChangesAsync(cancellation: Context.CancellationToken);
            return false;
        }

        return true;
    }

    public IConsumerContext Context { get; set; } = null!;
}
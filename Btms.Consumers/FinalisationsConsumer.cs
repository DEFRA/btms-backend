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
        var validationResult = validator.Validate(message);

        if (!validationResult.IsValid)
        {
            await dbContext.AlvsValidationErrors.Insert(
                new AlvsValidationError()
                {
                    Id = auditId,
                    Type = nameof(Finalisation),
                    Data = BsonDocument.Parse(GeneralExtensions.ToJson(message)),
                    ValidationResult = validationResult
                }, cancellationToken);
            await dbContext.SaveChangesAsync(cancellation: Context.CancellationToken);
        }

        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);
        var internalFinalisation = FinalisationMapper.Map(message);

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

    public IConsumerContext Context { get; set; } = null!;
}
using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Consumers.Extensions;
using Btms.Model.Cds;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Consumers;

public interface IDecisionsConsumer
{
    Task OnHandle(Decision message, IConsumerContext context, CancellationToken cancellationToken);
}

public class DecisionsConsumer(IMongoDbContext dbContext, MovementBuilderFactory movementBuilderFactory, ILogger<DecisionsConsumer> logger)
    : IDecisionsConsumer
{
    public async Task OnHandle(Decision message, IConsumerContext context, CancellationToken cancellationToken)
    {
        var internalDecision = DecisionMapper.Map(message);
        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);

        logger.LogInformation("OnHandle Decision SourceSystem {SourceSystem}, EntryReference {EntryReference} DecisionNumber {DecisionNumber}",
            message.ServiceHeader?.SourceSystem,
            message.Header.EntryReference,
            message.Header.DecisionNumber);

        if (existingMovement != null)
        {
            var auditId = context.GetMessageId();
            var notificationContext = context.Headers.GetValueOrDefault("notifications", null) as List<DecisionImportNotifications>;

            var existingMovementBuilder = movementBuilderFactory
                .From(existingMovement)
                .MergeDecision(auditId, internalDecision, notificationContext);

            if (existingMovementBuilder.HasChanges)
            {
                var movement = existingMovementBuilder.Build();
                logger.LogInformation("OnHandle HasChanges max btmsDecision number {MaxBtmsDecisionNumber}, max alvs decision number {MaxAlvsDecisionNumber} paired btms decision number {PairedBtmsDecisionNumber}, paired alvs decision number {PairedAlvsDecisionNumber}",
                    movement.Decisions.Max(d => d.Header.DecisionNumber),
                    movement.AlvsDecisionStatus.Decisions.Max(d => d.Decision.Header.DecisionNumber),
                    movement.AlvsDecisionStatus.Context.DecisionComparison?.BtmsDecisionNumber,
                    movement.AlvsDecisionStatus.Context.AlvsDecisionNumber);

                await dbContext.Movements.Update(movement, existingMovement._Etag, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                logger.LogInformation("OnHandle no changes to store");
            }
        }
    }
}
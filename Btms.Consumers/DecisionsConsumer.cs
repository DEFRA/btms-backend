using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Consumers.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using SlimMessageBus;

namespace Btms.Consumers;

public class DecisionsConsumer(IMongoDbContext dbContext, MovementBuilderFactory movementBuilderFactory)
    : IConsumer<Decision>, IConsumerWithContext
{
    public async Task OnHandle(Decision message)
    {
        var internalClearanceRequest = DecisionMapper.Map(message);
        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);

        if (existingMovement != null)
        {
            var auditId = Context.GetMessageId();
            var notificationContext = Context.Headers.GetValueOrDefault("notifications", null) as List<DecisionImportNotifications>;
            
            var existingMovementBuilder = movementBuilderFactory
                .From(existingMovement!)
                .MergeDecision(auditId!, internalClearanceRequest, notificationContext);
            
            if (existingMovementBuilder.HasChanges)
            {
                await dbContext.Movements.Update(existingMovementBuilder.Build(), existingMovement._Etag);
                await dbContext.SaveChangesAsync(Context.CancellationToken);
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
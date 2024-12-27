using Btms.Backend.Data;
using Btms.Model;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using SlimMessageBus;

namespace Btms.Consumers;

public class DecisionsConsumer(IMongoDbContext dbContext, MovementBuilder movementBuilder)
    : IConsumer<Decision>, IConsumerWithContext
{
    public async Task OnHandle(Decision message)
    {
        var internalClearanceRequest = DecisionMapper.Map(message);
        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);
        
        if (existingMovement != null)
        {
            var auditId = Context.Headers["messageId"].ToString();
            movementBuilder = movementBuilder
                .From(existingMovement!)
                .MergeDecision(auditId!, internalClearanceRequest);
                // .Build();

            // var merged = existingMovement.MergeDecision(auditId!, internalClearanceRequest);
            if (movementBuilder.HasChanges)
            {
                await dbContext.Movements.Update(movementBuilder.Build(), existingMovement._Etag);
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
using Btms.Backend.Data;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using SlimMessageBus;

namespace Btms.Consumers;

public class DecisionsConsumer(IMongoDbContext dbContext)
    : IConsumer<Decision>, IConsumerWithContext
{
    public async Task OnHandle(Decision message)
    {
        var internalClearanceRequest = DecisionMapper.Map(message);
        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);

        if (existingMovement != null)
        {
            var auditId = Context.Headers["messageId"].ToString();
            var merged = existingMovement.MergeDecision(auditId!, internalClearanceRequest);
            if (merged)
            {
                await dbContext.Movements.Update(existingMovement, existingMovement._Etag);
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
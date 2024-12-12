using Btms.Backend.Data;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using SlimMessageBus;

namespace Btms.Consumers;

public class DecisionsConsumer(IMongoDbContext dbContext)
    : IConsumer<AlvsClearanceRequest>, IConsumerWithContext
{
    public async Task OnHandle(AlvsClearanceRequest message)
    {
        var internalClearanceRequest = AlvsClearanceRequestMapper.Map(message);
        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);

        if (existingMovement != null)
        {
            var auditId = Context.Headers["messageId"].ToString();
            if (auditId == null || internalClearanceRequest == null) return;
            var merged = existingMovement.MergeDecision(auditId, internalClearanceRequest);
            if (merged)
            {
                await dbContext.Movements.Update(existingMovement, existingMovement._Etag);
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
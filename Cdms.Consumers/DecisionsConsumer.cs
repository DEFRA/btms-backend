using Cdms.Backend.Data;
using Cdms.Types.Alvs;
using Cdms.Types.Alvs.Mapping;
using SlimMessageBus;

namespace Cdms.Consumers
{
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
                var merged = existingMovement.MergeDecision(auditId!, internalClearanceRequest);
                if (merged)
                {
                    await dbContext.Movements.Update(existingMovement, existingMovement._Etag);
                }
            }
        }

        public IConsumerContext Context { get; set; } = null!;
    }
}
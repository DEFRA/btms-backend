using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using SlimMessageBus;

namespace Btms.Consumers;

public class DecisionsConsumer(IMongoDbContext dbContext, MovementBuilder existingMovementBuilder)
    : IConsumer<Decision>, IConsumerWithContext
{
    public async Task OnHandle(Decision message)
    {
        var internalClearanceRequest = DecisionMapper.Map(message);
        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);

        if (existingMovement != null)
        {
            var auditId = Context.Headers["messageId"].ToString();
            var notificationContext = Context.Headers.GetValueOrDefault("notifications", null) as List<DecisionImportNotifications>;
            
            existingMovementBuilder = existingMovementBuilder
                .From(existingMovement!)
                .MergeDecision(auditId!, internalClearanceRequest, notificationContext);
                // .Build();

            // var merged = existingMovement.MergeDecision(auditId!, internalClearanceRequest);
            if (existingMovementBuilder.HasChanges)
            {
                await dbContext.Movements.Update(existingMovementBuilder.Build(), existingMovement._Etag);
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
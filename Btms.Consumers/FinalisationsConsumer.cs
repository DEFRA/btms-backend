using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Consumers.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Consumers;

public class FinalisationsConsumer(IMongoDbContext dbContext,
    ILogger<FinalisationsConsumer> logger)
    : IConsumer<Finalisation>, IConsumerWithContext
{
    public async Task OnHandle(Finalisation message)
    {
        // var internalClearanceRequest = DecisionMapper.Map(message);
        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);
        
        if (existingMovement != null)
        {
            logger.LogInformation("Finalisation received");
            
            //     var auditId = Context.GetMessageId();
            //     var notificationContext = Context.Headers.GetValueOrDefault("notifications", null) as List<DecisionImportNotifications>;
            //     
            //     var existingMovementBuilder = movementBuilderFactory
            //         .From(existingMovement!)
            //         .MergeDecision(auditId!, internalClearanceRequest, notificationContext);
            //     
            //     if (existingMovementBuilder.HasChanges)
            //     {
            //         await dbContext.Movements.Update(existingMovementBuilder.Build(), existingMovement._Etag);
            //     }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Consumers.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using Finalisation = Btms.Types.Alvs.Finalisation;

namespace Btms.Consumers;

public class FinalisationsConsumer(IMongoDbContext dbContext,
    MovementBuilderFactory movementBuilderFactory, 
    ILogger<FinalisationsConsumer> logger)
    : IConsumer<Finalisation>, IConsumerWithContext
{
    public async Task OnHandle(Finalisation message)
    {
        var existingMovement = await dbContext.Movements.Find(message.Header!.EntryReference!);
        var internalFinalisation = FinalisationMapper.Map(message);
        
        if (existingMovement != null)
        {
            logger.LogInformation("Finalisation received");
            
            var auditId = Context.GetMessageId();
            
            var existingMovementBuilder = movementBuilderFactory
                .From(existingMovement!)
                .MergeFinalisation(auditId!, internalFinalisation);
            
            if (existingMovementBuilder.HasChanges)
            {
                await dbContext.Movements.Update(existingMovementBuilder.Build(), existingMovement._Etag);
                await dbContext.SaveChangesAsync(Context.CancellationToken);
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
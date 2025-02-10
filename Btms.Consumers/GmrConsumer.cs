using Btms.Backend.Data;
using Btms.Consumers.Extensions;
using Btms.Model.Auditing;
using Btms.Types.Gvms;
using Btms.Types.Gvms.Mapping;
using SlimMessageBus;
using SearchGmrsForDeclarationIdsResponse = Btms.Types.Gvms.SearchGmrsForDeclarationIdsResponse;

namespace Btms.Consumers;

internal class GmrConsumer(IMongoDbContext mongoDbContext)
    : IConsumer<SearchGmrsForDeclarationIdsResponse>, IConsumer<Gmr>, IConsumerWithContext
{
    public async Task OnHandle(SearchGmrsForDeclarationIdsResponse message, CancellationToken cancellationToken)
    {
        foreach (var gmr in message.Gmrs!)
        {
            await SaveOrUpdateGmr(gmr, Context.GetMessageId(), Context.CancellationToken);
        }

        await mongoDbContext.SaveChangesAsync(Context.CancellationToken);
    }
    
    public async Task OnHandle(Gmr message, CancellationToken cancellationToken)
    {
        await SaveOrUpdateGmr(message, Context.GetMessageId(), Context.CancellationToken);
        await mongoDbContext.SaveChangesAsync(Context.CancellationToken);
    }

    private async Task SaveOrUpdateGmr(Gmr message, string auditId, CancellationToken cancellationToken)
    {
        var mappedGmr = GrmWithTransformMapper.MapWithTransform(message);
        var existingGmr = await mongoDbContext.Gmrs.Find(mappedGmr.Id!);

        if (existingGmr is null)
        {
            var auditEntry = AuditEntry.CreateCreatedEntry(
                mappedGmr,
                auditId,
                1,
                message.UpdatedSource,
                CreatedBySystem.Gvms);

            mappedGmr.AuditEntries.Add(auditEntry);

            await mongoDbContext.Gmrs.Insert(mappedGmr, cancellationToken);
        }
        else
        {
            if (message.UpdatedSource > existingGmr.UpdatedSource)
            {
                mappedGmr.AuditEntries = existingGmr.AuditEntries;

                var auditEntry = AuditEntry.CreateUpdated(
                    existingGmr,
                    mappedGmr,
                    auditId,
                    mappedGmr.AuditEntries.Count + 1,
                    message.UpdatedSource,
                    CreatedBySystem.Gvms);

                mappedGmr.AuditEntries.Add(auditEntry);

                await mongoDbContext.Gmrs.Update(mappedGmr, existingGmr._Etag, cancellationToken);
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
using Btms.Backend.Data;
using Btms.Model.Auditing;
using Btms.Types.Gvms.Mapping;
using SlimMessageBus;
using SearchGmrsForDeclarationIdsResponse = Btms.Types.Gvms.SearchGmrsForDeclarationIdsResponse;

namespace Btms.Consumers;

internal class GmrConsumer(IMongoDbContext dbContext)
    : IConsumer<SearchGmrsForDeclarationIdsResponse>, IConsumerWithContext
{
    public async Task OnHandle(SearchGmrsForDeclarationIdsResponse message)
    {
        foreach (var gmr in message.Gmrs!)
        {
            var internalGmr = GrmWithTransformMapper.MapWithTransform(gmr);
            if (internalGmr == null) return;
            var existingGmr = await dbContext.Gmrs.Find(internalGmr.Id!);
            var auditId = Context.Headers["messageId"].ToString();
            if (existingGmr is null)
            {
                var auditEntry =
                    AuditEntry.CreateCreatedEntry(internalGmr, auditId!, 1, gmr.LastUpdated);
                internalGmr.AuditEntries.Add(auditEntry);
                await dbContext.Gmrs.Insert(internalGmr);
            }
            else
            {
                if (gmr.LastUpdated > existingGmr.LastUpdated)
                {
                    internalGmr.AuditEntries = existingGmr.AuditEntries;
                    var auditEntry = AuditEntry.CreateUpdated(
                        previous: existingGmr,
                        current: internalGmr,
                        id: auditId!,
                        version: internalGmr.AuditEntries.Count + 1,
                        lastUpdated: gmr.LastUpdated);
                    internalGmr.AuditEntries.Add(auditEntry);
                    await dbContext.Gmrs.Update(internalGmr, existingGmr._Etag);
                }
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
using Cdms.Backend.Data;
using Cdms.Model.Auditing;
using SlimMessageBus;
using Cdms.Model.Gvms;
using Cdms.Types.Gvms.Mapping;
using SearchGmrsForDeclarationIdsResponse = Cdms.Types.Gvms.SearchGmrsForDeclarationIdsResponse;

namespace Cdms.Business.Consumers
{
    internal class GmrConsumer(IMongoDbContext dbContext)
        : IConsumer<SearchGmrsForDeclarationIdsResponse>, IConsumerWithContext
    {
        public async Task OnHandle(SearchGmrsForDeclarationIdsResponse message)
        {
            foreach (var gmr in message.Gmrs)
            {
                var internalGmr = GmrMapper.Map(gmr);
                var existingGmr = await dbContext.Gmrs.Find(internalGmr.Id);

                if (existingGmr is null)
                {
                    var auditEntry =
                        AuditEntry.CreateCreatedEntry(internalGmr, internalGmr.Id, 1, gmr.LastUpdated);
                    internalGmr.AuditEntries.Add(auditEntry);
                    await dbContext.Gmrs.Insert(internalGmr);
                }
                else
                {
                    if (gmr.LastUpdated > existingGmr.LastUpdated)
                    {
                        internalGmr.AuditEntries = existingGmr.AuditEntries;
                        var auditEntry = AuditEntry.CreateUpdated<Gmr>(
                            previous: existingGmr,
                            current: internalGmr,
                            id: internalGmr.Id,
                            version: internalGmr.AuditEntries.Count + 1,
                            lastUpdated: gmr.LastUpdated);
                        internalGmr.AuditEntries.Add(auditEntry);
                        await dbContext.Gmrs.Update(internalGmr, existingGmr._Etag);
                    }
                }
            }
        }

        public IConsumerContext Context { get; set; }
    }
}
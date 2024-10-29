using Cdms.Backend.Data;
using Cdms.Model.Auditing;
using SlimMessageBus;
using System.Diagnostics.Metrics;
using Cdms.Model;
using Cdms.Model.VehicleMovement;
using Cdms.Types.Gmr.Mapping;
using SearchGmrsForDeclarationIdsResponse = Cdms.Types.Gmr.SearchGmrsForDeclarationIdsResponse;

namespace Cdms.Business.Consumers
{
    public class GmrMetrics
    {
        private readonly Counter<int> processed;

        public GmrMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("Cdms");
            processed = meter.CreateCounter<int>("cdms.gmr.processed");
        }

        public void MessageProcessed()
        {
            processed.Add(1);
        }
    }


    internal class GmrConsumer(IMongoDbContext dbContext, GmrMetrics metrics)
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
                        await dbContext.Gmrs.Update(internalGmr);
                    }
                }

                metrics.MessageProcessed();
            }
        }

        public IConsumerContext Context { get; set; }
    }
}
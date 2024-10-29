using Cdms.Backend.Data;
using Cdms.Model.Auditing;
using Cdms.Types.Ipaffs;
using Cdms.Types.Ipaffs.Mapping;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Cdms.Model;
using Cdms.Types.Alvs;
using Cdms.Types.Alvs.Mapping;

namespace Cdms.Business.Consumers
{
    public class AlvsClearanceRequestMetrics
    {
        private readonly Counter<int> processed;

        public AlvsClearanceRequestMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("Cdms");
            processed = meter.CreateCounter<int>("cdms.clearancerequest.processed");
        }

        public void MessageProcessed()
        {
            processed.Add(1);
        }
    }


    internal class AlvsClearanceRequestConsumer(IMongoDbContext dbContext, AlvsClearanceRequestMetrics metrics)
        : IConsumer<AlvsClearanceRequest>, IConsumerWithContext
    {
        public async Task OnHandle(AlvsClearanceRequest message)
        {
            var internalClearanceRequest = AlvsClearanceRequestMapper.Map(message);
            var auditId = Context.Headers["messageId"].ToString();
            var movement = BuildMovement(internalClearanceRequest);
            var existingMovement = await dbContext.Movements.Find(movement.Id);


            if (existingMovement is not null)
            {
                if (movement.ClearanceRequests.First().Header.EntryVersionNumber >
                    existingMovement.ClearanceRequests.First().Header.EntryVersionNumber)
                {
                    movement.AuditEntries = existingMovement.AuditEntries;
                    var auditEntry = AuditEntry.CreateUpdated(existingMovement.ClearanceRequests.First(),
                        movement.ClearanceRequests.First(),
                        BuildNormalizedAlvsPath(auditId),
                        movement.ClearanceRequests.First().Header.EntryVersionNumber.GetValueOrDefault(),
                        movement.LastUpdated);
                    movement.Update(auditEntry);

                    existingMovement.ClearanceRequests.RemoveAll(x =>
                        x.Header.EntryReference ==
                        movement.ClearanceRequests.First().Header.EntryReference);
                    existingMovement.ClearanceRequests.AddRange(movement.ClearanceRequests);

                    existingMovement.Items.AddRange(movement.Items);
                    await dbContext.Movements.Update(existingMovement);
                }
            }
            else
            {
                var auditEntry = AuditEntry.CreateCreatedEntry(
                    movement.ClearanceRequests.First(),
                    BuildNormalizedAlvsPath(auditId),
                    movement.ClearanceRequests.First().Header.EntryVersionNumber.GetValueOrDefault(),
                    movement.LastUpdated);
                movement.Update(auditEntry);
                await dbContext.Movements.Insert(movement);
                existingMovement = await dbContext.Movements.Find(movement.Id);
            }


            metrics.MessageProcessed();
        }

        public IConsumerContext Context { get; set; }

        public static Movement BuildMovement(Model.Alvs.AlvsClearanceRequest request)
        {
            return new Movement()
            {
                Id = request.Header!.EntryReference,
                LastUpdated = request.ServiceHeader?.ServiceCalled,
                EntryReference = request.Header.EntryReference,
                MasterUCR = request.Header.MasterUcr,
                // DeclarationPartNumber = ConvertInt(r.Header.DeclarationPartNumber),
                DeclarationType = request.Header.DeclarationType,
                // ArrivalDateTime = r.Header.ArrivalDateTime,
                SubmitterTURN = request.Header.SubmitterTurn,
                DeclarantId = request.Header.DeclarantId,
                DeclarantName = request.Header.DeclarantName,
                DispatchCountryCode = request.Header.DispatchCountryCode,
                GoodsLocationCode = request.Header.GoodsLocationCode,
                ClearanceRequests = new List<Model.Alvs.AlvsClearanceRequest>() { request },
                Items = request.Items?.Select(x => { return x; }).ToList(),
            };
        }

        private string BuildNormalizedAlvsPath(string fullPath)
        {
            return fullPath.Replace("RAW/ALVS/", "");
        }
    }
}
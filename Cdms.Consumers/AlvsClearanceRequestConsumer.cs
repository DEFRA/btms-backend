using Cdms.Backend.Data;
using Cdms.Model;
using Cdms.Model.Auditing;
using Cdms.Types.Alvs;
using Cdms.Types.Alvs.Mapping;
using SlimMessageBus;
using Items = Cdms.Model.Alvs.Items;

namespace Cdms.Consumers
{
    internal class AlvsClearanceRequestConsumer(IMongoDbContext dbContext)
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
                if (movement.ClearanceRequests[0].Header.EntryVersionNumber >
                    existingMovement.ClearanceRequests[0].Header.EntryVersionNumber)
                {
                    movement.AuditEntries = existingMovement.AuditEntries;
                    var auditEntry = AuditEntry.CreateUpdated(existingMovement.ClearanceRequests[0],
                        movement.ClearanceRequests[0],
                        BuildNormalizedAlvsPath(auditId),
                        movement.ClearanceRequests[0].Header.EntryVersionNumber.GetValueOrDefault(),
                        movement.LastUpdated);
                    movement.Update(auditEntry);

                    existingMovement.ClearanceRequests.RemoveAll(x =>
                        x.Header.EntryReference ==
                        movement.ClearanceRequests[0].Header.EntryReference);
                    existingMovement.ClearanceRequests.AddRange(movement.ClearanceRequests);
                    if (existingMovement.Items == null)
                    {
                        existingMovement.Items = new List<Items>();
                    }

                    existingMovement.Items.AddRange(movement.Items);
                    await dbContext.Movements.Update(existingMovement, existingMovement._Etag);
                }
            }
            else
            {
                var auditEntry = AuditEntry.CreateCreatedEntry(
                    movement.ClearanceRequests[0],
                    BuildNormalizedAlvsPath(auditId),
                    movement.ClearanceRequests[0].Header.EntryVersionNumber.GetValueOrDefault(),
                    movement.LastUpdated);
                movement.Update(auditEntry);
                await dbContext.Movements.Insert(movement);
            }
        }

        public IConsumerContext Context { get; set; }

        public static Movement BuildMovement(Model.Alvs.AlvsClearanceRequest request)
        {
            return new Movement()
            {
                Id = request.Header!.EntryReference,
                LastUpdated = request.ServiceHeader?.ServiceCalled,
                EntryReference = request.Header.EntryReference,
                MasterUcr = request.Header.MasterUcr,
                // DeclarationPartNumber = ConvertInt(r.Header.DeclarationPartNumber),
                DeclarationType = request.Header.DeclarationType,
                // ArrivalDateTime = r.Header.ArrivalDateTime,
                SubmitterTurn = request.Header.SubmitterTurn,
                DeclarantId = request.Header.DeclarantId,
                DeclarantName = request.Header.DeclarantName,
                DispatchCountryCode = request.Header.DispatchCountryCode,
                GoodsLocationCode = request.Header.GoodsLocationCode,
                ClearanceRequests = new List<Model.Alvs.AlvsClearanceRequest>() { request },
                Items = request.Items?.Select(x => { return x; }).ToList(),
            };
        }

        private static string BuildNormalizedAlvsPath(string fullPath)
        {
            return fullPath.Replace("RAW/ALVS/", "");
        }
    }
}
using Btms.Backend.Data;
using Btms.Business.Services;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using System.Diagnostics.CodeAnalysis;
using Btms.Consumers.Extensions;
using Force.DeepCloner;
using Items = Btms.Model.Alvs.Items;

namespace Btms.Consumers
{
    internal class AlvsClearanceRequestConsumer(IMongoDbContext dbContext, ILinkingService linkingService, ILogger<AlvsClearanceRequestConsumer> logger)
        : IConsumer<AlvsClearanceRequest>, IConsumerWithContext
    {
        private ILinkingService linkingService { get; } = linkingService;

        [SuppressMessage("SonarLint", "S1481",
            Justification =
                "LinkResult variable is unused until matching and decisions are implemented")]
        public async Task OnHandle(AlvsClearanceRequest message)
        {
            var auditId = Context.Headers["messageId"].ToString();
            logger.ConsumerStarted(Context.GetJobId()!, auditId!, GetType().Name, message.Header?.EntryReference!);
            using (logger.BeginScope(new List<KeyValuePair<string, object>>
                   {
                       new("JobId", Context.GetJobId()!),
                       new("MessageId", auditId!),
                       new("Consumer", GetType().Name),
                       new("Identifier", message.Header?.EntryReference!),
                   }))
            {
                var internalClearanceRequest = AlvsClearanceRequestMapper.Map(message);
                var movement = BuildMovement(internalClearanceRequest);
                var existingMovement = await dbContext.Movements.Find(movement.Id!);
                Movement persistedMovement = null!;

                if (existingMovement is not null)
                {
                    persistedMovement = existingMovement.DeepClone();
                    if (movement.ClearanceRequests[0].Header?.EntryVersionNumber >
                        existingMovement.ClearanceRequests[0].Header?.EntryVersionNumber)
                    {
                        ////movement.AuditEntries = existingMovement.AuditEntries;
                        var auditEntry = AuditEntry.CreateUpdated(existingMovement.ClearanceRequests[0],
                            movement.ClearanceRequests[0],
                            BuildNormalizedAlvsPath(auditId!),
                            movement.ClearanceRequests[0].Header!.EntryVersionNumber.GetValueOrDefault(),
                            movement.UpdatedSource);
                        movement.Update(auditEntry);

                        existingMovement.ClearanceRequests.RemoveAll(x =>
                            x.Header?.EntryReference ==
                            movement.ClearanceRequests[0].Header?.EntryReference);
                        existingMovement.ClearanceRequests.AddRange(movement.ClearanceRequests);
                        if (existingMovement.Items == null)
                        {
                            existingMovement.Items = new List<Items>();
                        }

                        existingMovement.Items.AddRange(movement.Items);
                        await dbContext.Movements.Update(existingMovement, existingMovement._Etag);
                    }
                    else
                    {
                        logger.MessageSkipped(Context.GetJobId()!, auditId!, GetType().Name, message.Header?.EntryReference!);
                        Context.Skipped();
                    }
                }
                else
                {
                    persistedMovement = movement!;
                    var auditEntry = AuditEntry.CreateCreatedEntry(
                        movement.ClearanceRequests[0],
                        BuildNormalizedAlvsPath(auditId!),
                        movement.ClearanceRequests[0].Header!.EntryVersionNumber.GetValueOrDefault(),
                        movement.UpdatedSource);
                    movement.Update(auditEntry);
                    await dbContext.Movements.Insert(movement);
                }

                //This should be existing, pre update (may need to clone)
                var linkContext = new MovementLinkContext(persistedMovement, existingMovement);
                var linkResult = await linkingService.Link(linkContext, Context.CancellationToken);
            }
        }

        public IConsumerContext Context { get; set; } = null!;

        public static Movement BuildMovement(Model.Alvs.AlvsClearanceRequest request)
        {
            return new Movement()
            {
                Id = request.Header!.EntryReference,
                UpdatedSource = request.ServiceHeader?.ServiceCalled,
                CreatedSource = request.ServiceHeader?.ServiceCalled,
                ArrivesAt = request.Header.ArrivesAt, 
                EntryReference = request.Header.EntryReference!,
                MasterUcr = request.Header.MasterUcr!,
                DeclarationType = request.Header.DeclarationType!,
                SubmitterTurn = request.Header.SubmitterTurn!,
                DeclarantId = request.Header.DeclarantId!,
                DeclarantName = request.Header.DeclarantName!,
                DispatchCountryCode = request.Header.DispatchCountryCode!,
                GoodsLocationCode = request.Header.GoodsLocationCode!,
                ClearanceRequests = [request],
                Items = request.Items?.Select(x => x).ToList()!,
            };
        }

        private static string BuildNormalizedAlvsPath(string fullPath)
        {
            return fullPath.Replace("RAW/ALVS/", "");
        }
    }
}
using Btms.Backend.Data;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Model.ChangeLog;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Pipelines.PreProcessing;

public class MovementPreProcessor(IMongoDbContext dbContext, ILogger<MovementPreProcessor> logger) : IPreProcessor<AlvsClearanceRequest, Movement>
{
    public async Task<PreProcessingResult<Movement>> Process(PreProcessingContext<AlvsClearanceRequest> preProcessingContext)
    {

        var internalClearanceRequest = AlvsClearanceRequestMapper.Map(preProcessingContext.Message);
        ArgumentNullException.ThrowIfNull(internalClearanceRequest);
        var movement = BuildMovement(internalClearanceRequest);
        var existingMovement = await dbContext.Movements.Find(movement.Id!);

        if (existingMovement is null)
        {
            ArgumentNullException.ThrowIfNull(movement);
            var auditEntry = AuditEntry.CreateCreatedEntry(
                movement.ClearanceRequests[0],
                preProcessingContext.MessageId,
                movement.ClearanceRequests[0].Header?.EntryVersionNumber.GetValueOrDefault() ?? -1,
                movement.UpdatedSource);
            movement.Update(auditEntry);
            await dbContext.Movements.Insert(movement);
            return PreProcessResult.New(movement);
        }

        if (movement.ClearanceRequests[^1].Header?.EntryVersionNumber > existingMovement.ClearanceRequests[0].Header?.EntryVersionNumber)
        {
            var changeSet = movement.ClearanceRequests[^1].GenerateChangeSet(existingMovement.ClearanceRequests[0]);


            var auditEntry = AuditEntry.CreateUpdated(changeSet,
                preProcessingContext.MessageId,
                movement.ClearanceRequests[0].Header!.EntryVersionNumber.GetValueOrDefault(),
                movement.UpdatedSource);
            movement.Update(auditEntry);

            existingMovement.ClearanceRequests.RemoveAll(x =>
                x.Header?.EntryReference ==
                movement.ClearanceRequests[0].Header?.EntryReference);
            existingMovement.ClearanceRequests.AddRange(movement.ClearanceRequests);

            existingMovement.Items.AddRange(movement.Items);
            await dbContext.Movements.Update(existingMovement, existingMovement._Etag);
            return PreProcessResult.Changed(existingMovement, changeSet);
        }

        if (movement.ClearanceRequests[^1].Header?.EntryVersionNumber ==
            existingMovement.ClearanceRequests[0].Header?.EntryVersionNumber)
        {
            return PreProcessResult.AlreadyProcessed(existingMovement);
        }

        logger.MessageSkipped(preProcessingContext.MessageId, preProcessingContext.Message.Header?.EntryReference!);
        return PreProcessResult.Skipped(existingMovement);
        
    }

    public static Movement BuildMovement(Model.Alvs.AlvsClearanceRequest request)
    {
        return new Movement
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
}
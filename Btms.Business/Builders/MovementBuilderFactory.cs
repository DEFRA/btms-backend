using Btms.Common.Extensions;
using Btms.Model.Cds;
using Microsoft.Extensions.Logging;
using Btms.Business.Extensions;
using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Builders;

public class MovementBuilderFactory(DecisionStatusFinder decisionStatusFinder, ILogger<MovementBuilder> logger)
{
    public MovementBuilder From(CdsClearanceRequest request)
    {
        logger.LogInformation("Creating movement from clearance request {0}", request.Header!.EntryReference);
        var items = request.Items?.ToList()!;
        var documentReferenceIds = items.UniqueDocumentReferenceIds();
        var notificationRelationshipIds = new List<string>();

        var movement = new Movement()
        {
            Id = request.Header!.EntryReference,
            UpdatedSource = request.ServiceHeader?.ServiceCalled,
            CreatedSource = request.ServiceHeader?.ServiceCalled,
            ArrivesAt = request.Header.ArrivesAt,
            EntryReference = request.Header.EntryReference!,
            EntryVersionNumber = request.Header.EntryVersionNumber.GetValueOrDefault(),
            MasterUcr = request.Header.MasterUcr!,
            DeclarationType = request.Header.DeclarationType!,
            SubmitterTurn = request.Header.SubmitterTurn!,
            DeclarantId = request.Header.DeclarantId!,
            DeclarantName = request.Header.DeclarantName!,
            DispatchCountryCode = request.Header.DispatchCountryCode!,
            GoodsLocationCode = request.Header.GoodsLocationCode!,
            ClearanceRequests = [request],
            Items = items!.AsInternalItems(),
            Status = MovementExtensions.GetMovementStatus(GetChedTypes(request.Items!.ToList()), documentReferenceIds, notificationRelationshipIds)
        };

        return new MovementBuilder(logger, decisionStatusFinder, movement, true);
    }

    public MovementBuilder From(Movement movement)
    {
        return new MovementBuilder(logger, decisionStatusFinder, movement, true);
    }

    private static ImportNotificationTypeEnum[] GetChedTypes(List<ClearanceRequestItems>? items = null)
    {
        return items?
            .SelectMany(i => i.Documents!)
            .Select(d =>
                d.DocumentCode!.GetChedType()
            )
            .Distinct()
            .Where(ct => ct.HasValue())
            .Select(ct => ct!.Value)
            .ToArray()!;
    }
}
using Microsoft.Extensions.Logging;

namespace Btms.Model;


public class MovementBuilder(ILogger<MovementBuilder> logger)
{
    private Movement? _movement;
    
    public MovementBuilder From(Model.Cds.CdsClearanceRequest request)
    {
        logger.LogInformation("Creating movement from clearance request {0}", request.Header!.EntryReference);
        
        _movement = new Movement()
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
            Items = request.Items?.Select(x => x).ToList()!,
        };
        
        return (MovementBuilder)this;
    }

    public Movement Build()
    {
        return _movement!;
    }
}
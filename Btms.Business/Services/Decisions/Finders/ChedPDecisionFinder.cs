using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedPDecisionFinder : IDecisionFinder
{
    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        if (consignmentAcceptable is true)
        {
            return notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForTranshipment or 
                    DecisionDecisionEnum.AcceptableForTransit or 
                    DecisionDecisionEnum.AcceptableForSpecificWarehouse => new DecisionFinderResult(DecisionCode.E03),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03),
                DecisionDecisionEnum.AcceptableIfChanneled => new DecisionFinderResult(DecisionCode.C06), 
                _ => throw new ArgumentOutOfRangeException(nameof(notification),
                    "Decision type unexpected for cheda when consignment acceptable")
            };
        }

        if (consignmentAcceptable is false)
        {
            return notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction  => new DecisionFinderResult(DecisionCode.N02),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07),
                _ => throw new ArgumentOutOfRangeException(nameof(notification),
                    "Decision type unexpected for cheda when consignment acceptable")
            };
        }
        return new DecisionFinderResult(DecisionCode.C03);
    }
}
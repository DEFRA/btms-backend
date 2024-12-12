using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedADecisionFinder : IDecisionFinder
{
    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        if (consignmentAcceptable is true)
        {
            return notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForTranshipment or DecisionDecisionEnum.AcceptableForTransit =>
                    new DecisionFinderResult(DecisionCode.E03),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03),
                DecisionDecisionEnum.AcceptableForTemporaryImport => new DecisionFinderResult(DecisionCode.C05),
                DecisionDecisionEnum.HorseReEntry => new DecisionFinderResult(DecisionCode.C06),
                _ => throw new ArgumentOutOfRangeException(nameof(notification),
                    "Decision type unexpected for cheda when consignment acceptable")
            };
        }

        if (consignmentAcceptable is false)
        {
            return notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Euthanasia or DecisionNotAcceptableActionEnum.Slaughter =>
                    new DecisionFinderResult(DecisionCode.N02),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04),
                _ => throw new ArgumentOutOfRangeException(nameof(notification),
                    "Decision type unexpected for cheda when consignment acceptable")
            };
        }
        return new DecisionFinderResult(DecisionCode.C03);
    }
}
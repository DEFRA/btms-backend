using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedDDecisionFinder : IDecisionFinder
{
    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value);
        }

        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        return consignmentAcceptable switch
        {
            true => notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03),
                _ => new DecisionFinderResult(DecisionCode.E96)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02),
                DecisionNotAcceptableActionEnum.Redispatching => new DecisionFinderResult(DecisionCode.N04),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07),
                _ => new DecisionFinderResult(DecisionCode.E97)
            },
            _ => new DecisionFinderResult(DecisionCode.E99)
        };
    }
}
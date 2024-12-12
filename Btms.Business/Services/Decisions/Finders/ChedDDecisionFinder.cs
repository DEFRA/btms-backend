using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedDDecisionFinder : IDecisionFinder
{
    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        if (consignmentAcceptable is true)
        {
            return notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03),
                _ => throw new ArgumentOutOfRangeException(nameof(notification),
                    "Decision type unexpected for cheda when consignment acceptable")
            };
        }

        if (consignmentAcceptable is false)
        {
            return notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02),
                DecisionNotAcceptableActionEnum.Redispatching => new DecisionFinderResult(DecisionCode.N04),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07),
                _ => throw new ArgumentOutOfRangeException(nameof(notification),
                    "Decision type unexpected for cheda when consignment acceptable")
            };
        }
        return new DecisionFinderResult(DecisionCode.C03);
    }
}


public class NoMatchFinder : IDecisionFinder
{
    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        if (consignmentAcceptable is true)
        {
            return notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03),
                _ => throw new ArgumentOutOfRangeException(nameof(notification),
                    "Decision type unexpected for cheda when consignment acceptable")
            };
        }

        if (consignmentAcceptable is false)
        {
            return notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02),
                DecisionNotAcceptableActionEnum.Redispatching => new DecisionFinderResult(DecisionCode.N04),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07),
                _ => throw new ArgumentOutOfRangeException(nameof(notification),
                    "Decision type unexpected for cheda when consignment acceptable")
            };
        }
        return new DecisionFinderResult(DecisionCode.C03);
    }
}
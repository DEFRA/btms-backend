using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedPDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification) => notification.ImportNotificationType == ImportNotificationTypeEnum.Cvedp;

    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value, DecisionType.Ched);
        }

        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        return consignmentAcceptable switch
        {
            true => notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForTranshipment or DecisionDecisionEnum.AcceptableForTransit
                    or DecisionDecisionEnum.AcceptableForSpecificWarehouse =>
                    new DecisionFinderResult(DecisionCode.E03, DecisionType.Ched),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03, DecisionType.Ched),
                DecisionDecisionEnum.AcceptableIfChanneled => new DecisionFinderResult(DecisionCode.C06, DecisionType.Ched),
                _ => new DecisionFinderResult(DecisionCode.X00, DecisionType.Ched)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02, DecisionType.Ched),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04, DecisionType.Ched),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03, DecisionType.Ched),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07, DecisionType.Ched),
                _ => new DecisionFinderResult(DecisionCode.X00, DecisionType.Ched)
            },
            _ => new DecisionFinderResult(DecisionCode.X00, DecisionType.Ched)
        };
    }
}
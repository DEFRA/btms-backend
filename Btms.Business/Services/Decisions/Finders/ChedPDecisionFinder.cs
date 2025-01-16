using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedPDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification) => notification.PartTwo?.ControlAuthority?.IuuCheckRequired == false && notification.ImportNotificationType == ImportNotificationTypeEnum.Cvedp;

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
                DecisionDecisionEnum.AcceptableForTranshipment or DecisionDecisionEnum.AcceptableForTransit
                    or DecisionDecisionEnum.AcceptableForSpecificWarehouse =>
                    new DecisionFinderResult(DecisionCode.E03),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03),
                DecisionDecisionEnum.AcceptableIfChanneled => new DecisionFinderResult(DecisionCode.C06),
                _ => new DecisionFinderResult(DecisionCode.X00)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07),
                _ => new DecisionFinderResult(DecisionCode.X00)
            },
            _ => new DecisionFinderResult(DecisionCode.X00)
        };
    }
}
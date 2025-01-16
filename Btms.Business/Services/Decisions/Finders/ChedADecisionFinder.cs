using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedADecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification) => notification.ImportNotificationType == ImportNotificationTypeEnum.Cveda;

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
                DecisionDecisionEnum.AcceptableForTranshipment or DecisionDecisionEnum.AcceptableForTransit => new DecisionFinderResult(DecisionCode.E03),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03),
                DecisionDecisionEnum.AcceptableForTemporaryImport => new DecisionFinderResult(DecisionCode.C05),
                DecisionDecisionEnum.HorseReEntry => new DecisionFinderResult(DecisionCode.C06),
                _ => new DecisionFinderResult(DecisionCode.X00)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Euthanasia or DecisionNotAcceptableActionEnum.Slaughter => new DecisionFinderResult(DecisionCode.N02),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04),
                _ => new DecisionFinderResult(DecisionCode.X00)
            },
            _ => new DecisionFinderResult(DecisionCode.X00)
        };
    }
}
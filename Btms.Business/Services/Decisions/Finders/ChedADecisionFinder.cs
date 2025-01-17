using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedADecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification) => notification.ImportNotificationType == ImportNotificationTypeEnum.Cveda;

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
                DecisionDecisionEnum.AcceptableForTranshipment or DecisionDecisionEnum.AcceptableForTransit => new DecisionFinderResult(DecisionCode.E03, DecisionType.Ched),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03, DecisionType.Ched),
                DecisionDecisionEnum.AcceptableForTemporaryImport => new DecisionFinderResult(DecisionCode.C05, DecisionType.Ched),
                DecisionDecisionEnum.HorseReEntry => new DecisionFinderResult(DecisionCode.C06, DecisionType.Ched),
                _ => new DecisionFinderResult(DecisionCode.E96, DecisionType.Ched)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Euthanasia or DecisionNotAcceptableActionEnum.Slaughter => new DecisionFinderResult(DecisionCode.N02, DecisionType.Ched),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04, DecisionType.Ched),
                _ => new DecisionFinderResult(DecisionCode.E97, DecisionType.Ched)
            },
            _ => new DecisionFinderResult(DecisionCode.E99, DecisionType.Ched)
        };
    }
}
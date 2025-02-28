using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedADecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification, string? checkCode) => notification.ImportNotificationType == ImportNotificationTypeEnum.Cveda && notification.PartTwo?.ControlAuthority?.IuuCheckRequired != true;

    public DecisionFinderResult FindDecision(ImportNotification notification, string? checkCode)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value, checkCode);
        }

        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        return consignmentAcceptable switch
        {
            true => notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForTranshipment or DecisionDecisionEnum.AcceptableForTransit => new DecisionFinderResult(DecisionCode.E03, checkCode),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03, checkCode),
                DecisionDecisionEnum.AcceptableForTemporaryImport => new DecisionFinderResult(DecisionCode.C05, checkCode),
                DecisionDecisionEnum.HorseReEntry => new DecisionFinderResult(DecisionCode.C06, checkCode),
                _ => new DecisionFinderResult(DecisionCode.X00, checkCode, InternalDecisionCode: DecisionInternalFurtherDetail.E96)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Euthanasia or DecisionNotAcceptableActionEnum.Slaughter => new DecisionFinderResult(DecisionCode.N02, checkCode),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04, checkCode),
                _ => new DecisionFinderResult(DecisionCode.X00, checkCode, InternalDecisionCode: DecisionInternalFurtherDetail.E97)
            },
            _ => new DecisionFinderResult(DecisionCode.X00, checkCode, InternalDecisionCode: DecisionInternalFurtherDetail.E99)
        };
    }
}
using Btms.Business.Extensions;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;



public class ChedDDecisionFinder : DecisionFinder
{
    public override bool CanFindDecision(ImportNotification notification, string? checkCode) =>
        notification.ImportNotificationType == ImportNotificationTypeEnum.Ced &&
        notification.PartTwo?.ControlAuthority?.IuuCheckRequired != true &&
        checkCode?.GetChedTypeFromCheckCode() == ImportNotificationTypeEnum.Ced;

    protected override DecisionFinderResult FindDecisionInternal(ImportNotification notification, string? checkCode)
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
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03,
                    checkCode),
                _ => new DecisionFinderResult(DecisionCode.X00, checkCode,
                    InternalDecisionCode: DecisionInternalFurtherDetail.E96)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02, checkCode),
                DecisionNotAcceptableActionEnum.Redispatching => new DecisionFinderResult(DecisionCode.N04, checkCode),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03, checkCode),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07, checkCode),
                null => notification.HandleNullNotAcceptableAction(checkCode),
                _ => new DecisionFinderResult(DecisionCode.X00, checkCode,
                    InternalDecisionCode: DecisionInternalFurtherDetail.E97)
            },
            _ => new DecisionFinderResult(DecisionCode.X00, checkCode,
                InternalDecisionCode: DecisionInternalFurtherDetail.E99)
        };
    }
}
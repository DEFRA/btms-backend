using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedDDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification, string[]? checkCodes = null) => notification.ImportNotificationType == ImportNotificationTypeEnum.Ced && notification.PartTwo?.ControlAuthority?.IuuCheckRequired != true;

    public DecisionFinderResult FindDecision(ImportNotification notification, string[]? checkCodes = null)
    {
        var checkCode = checkCodes?[0];

        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value, checkCode);
        }

        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        return consignmentAcceptable switch
        {
            true => notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03, checkCode),
                _ => new DecisionFinderResult(DecisionCode.E96, checkCode)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02, checkCode),
                DecisionNotAcceptableActionEnum.Redispatching => new DecisionFinderResult(DecisionCode.N04, checkCode),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03, checkCode),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07, checkCode),
                _ => new DecisionFinderResult(DecisionCode.E97, checkCode)
            },
            _ => new DecisionFinderResult(DecisionCode.E99, checkCode)
        };
    }
}
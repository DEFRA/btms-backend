using Btms.Common.Extensions;
using Btms.Model.Ipaffs;
using static Btms.Common.Extensions.LinksBuilder;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedPDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification, string? checkCode) =>
        notification.ImportNotificationType == ImportNotificationTypeEnum.Cvedp &&
        checkCode != IuuDecisionFinder.IuuCheckCode;

    public DecisionFinderResult FindDecision(ImportNotification notification, string? checkCode)
    {
        if (notification.Status == ImportNotificationStatusEnum.Cancelled ||
            notification.Status == ImportNotificationStatusEnum.Replaced)
        {
            return new DecisionFinderResult(DecisionCode.X00, checkCode, InternalDecisionCode: DecisionInternalFurtherDetail.E88);
        }

        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value, checkCode);
        }

        if (!notification.TryGetConsignmentAcceptable(out var consignmentAcceptable, out var decisionCode,
                out var internalDecisionCode))
        {
            return new DecisionFinderResult(decisionCode!.Value, checkCode, InternalDecisionCode: internalDecisionCode);
        }

        return consignmentAcceptable switch
        {
            true => notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForTranshipment or DecisionDecisionEnum.AcceptableForTransit
                    or DecisionDecisionEnum.AcceptableForSpecificWarehouse =>
                    new DecisionFinderResult(DecisionCode.E03, checkCode),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03,
                    checkCode),
                DecisionDecisionEnum.AcceptableIfChanneled => new DecisionFinderResult(DecisionCode.C06, checkCode),
                _ => new DecisionFinderResult(DecisionCode.X00, checkCode,
                    InternalDecisionCode: DecisionInternalFurtherDetail.E96)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02, checkCode),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04, checkCode),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03, checkCode),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07, checkCode),
                null => notification.HandleNullNotAcceptableAction(checkCode),
                _ => new DecisionFinderResult(DecisionCode.X00, checkCode,
                    InternalDecisionCode: DecisionInternalFurtherDetail.E97)
            }
        };
    }
}
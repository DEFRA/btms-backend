using Btms.Common.Extensions;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

internal static class DecisionFinderExtensions
{
    public static bool TryGetConsignmentAcceptable(this ImportNotification notification, out bool acceptable, out DecisionCode? decisionCode, out DecisionInternalFurtherDetail? internalDecisionCode)
    {
        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        decisionCode = null;
        internalDecisionCode = null;
        acceptable = false;

        if (consignmentAcceptable.HasValue)
        {
            acceptable = consignmentAcceptable.Value;
            return true;
        }
        else if (notification.PartTwo != null && notification.PartTwo.AutoClearedOn.HasValue())
        {
            acceptable = true;
            return true;
        }

        decisionCode = DecisionCode.X00;
        internalDecisionCode = DecisionInternalFurtherDetail.E99;
        return false;
    }

    public static bool TryGetHoldDecision(this ImportNotification notification, out DecisionCode? decisionCode)
    {
        if (notification.Status is ImportNotificationStatusEnum.Submitted or ImportNotificationStatusEnum.InProgress)
        {

            if (notification.PartTwo?.InspectionRequired is InspectionRequiredEnum.NotRequired or InspectionRequiredEnum.Inconclusive)
            {
                decisionCode = DecisionCode.H01;
                return true;
            }

            if (notification.PartTwo?.InspectionRequired == InspectionRequiredEnum.Required ||
                notification.Commodities.Any(x => x.RiskAssesment?.HmiDecision == CommodityRiskResultHmiDecisionEnum.Required) ||
                notification.Commodities.Any(x => x.RiskAssesment?.PhsiDecision == CommodityRiskResultPhsiDecisionEnum.Required))
            {
                decisionCode = DecisionCode.H02;
                return true;
            }

        }

        decisionCode = null;
        return false;
    }

    public static bool HasChecks(this DecisionContext decisionContext, string movementId, int itemNumber)
    {
        var checks = decisionContext.Movements.First(x => x.Id == movementId).Items
            .First(x => x.ItemNumber == itemNumber).Checks;
        return checks != null && checks.Any();
    }

    public static DecisionFinderResult HandleNullNotAcceptableAction(this ImportNotification notification, string? checkCode)
    {
        if (notification.PartTwo?.Decision?.NotAcceptableReasons?.Length > 0)
        {
            return new DecisionFinderResult(DecisionCode.N04, checkCode);
        }

        return new DecisionFinderResult(DecisionCode.X00, checkCode, InternalDecisionCode: DecisionInternalFurtherDetail.E97);
    }
}
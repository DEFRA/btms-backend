using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

internal static class DecisionFinderExtensions
{
    public static bool TryGetHoldDecision(this ImportNotification notification, out DecisionCode? decisionCode)
    {
        if (notification.Status == ImportNotificationStatusEnum.Submitted ||
            notification.Status == ImportNotificationStatusEnum.InProgress)
        {
            if (notification.PartTwo?.InspectionRequired == InspectionRequiredEnum.NotRequired || notification.PartTwo?.InspectionRequired == InspectionRequiredEnum.Inconclusive)
            {
                decisionCode = DecisionCode.H01;
                return true;
            }
            
            if (notification.PartTwo?.InspectionRequired == InspectionRequiredEnum.Required || notification.RiskAssessment?.CommodityResults?.Any(x => x.HmiDecision == CommodityRiskResultHmiDecisionEnum.Required) is true
                                                                       || notification.RiskAssessment?.CommodityResults?.Any(x => x.PhsiDecision == CommodityRiskResultPhsiDecisionEnum.Required) is true)
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
}
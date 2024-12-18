using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

internal static class DecisionFinderExtensions
{
    public static bool TryGetHoldDecision(this ImportNotification notification, out DecisionCode? decisionCode)
    {
        if (notification.Status == ImportNotificationStatusEnum.Submitted ||
            notification.Status == ImportNotificationStatusEnum.InProgress)
        {
            if (notification.PartTwo?.InspectionRequired == "NOTREQUIRED" || notification.PartTwo?.InspectionRequired == "INCONCLUSIVE")
            {
                decisionCode = DecisionCode.H01;
                return true;
            }
            
            if (notification.PartTwo?.InspectionRequired == "REQUIRED" || notification.RiskAssessment?.CommodityResults?.Any(x => x.HmiDecision == CommodityRiskResultHmiDecisionEnum.Required) is true
                                                                       || notification.RiskAssessment?.CommodityResults?.Any(x => x.PhsiDecision == CommodityRiskResultPhsiDecisionEnum.Required) is true)
            {
                decisionCode = DecisionCode.H02;
                return true;
            }

        }

        decisionCode = null;
        return false;
    }
}
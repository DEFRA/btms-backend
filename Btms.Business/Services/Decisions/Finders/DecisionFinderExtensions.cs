using Btms.Common.Extensions;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

internal static class DecisionFinderExtensions
{
    public static bool TryGetConsignmentAcceptable(this ImportNotification notification, out bool acceptable, out DecisionCode? decisionCode)
    {
        var consignmentAcceptable = notification.PartTwo?.Decision?.ConsignmentAcceptable;
        decisionCode = null;
        acceptable = false;
        
        if (consignmentAcceptable.HasValue)
        {
            acceptable = consignmentAcceptable.Value; 
            return true;
        }
        else if (notification.PartTwo!.AutoClearedOn!.HasValue())
        {
            acceptable = true;
            return true;
        }

        decisionCode = DecisionCode.E99;
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
                notification.RiskAssessment?.CommodityResults?.Any(x => x.HmiDecision == CommodityRiskResultHmiDecisionEnum.Required) is true
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
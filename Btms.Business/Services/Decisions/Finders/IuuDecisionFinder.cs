using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class IuuDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification, string[]? checkCodes) => notification.ImportNotificationType == ImportNotificationTypeEnum.Cvedp && checkCodes?.Contains("H224") == true;

    public DecisionFinderResult FindDecision(ImportNotification notification, string[]? _ = null)
    {
        return (notification.PartTwo?.ControlAuthority?.IuuCheckRequired == true) switch
        {
            true => notification.PartTwo?.ControlAuthority?.IuuOption switch
            {
                ControlAuthorityIuuOptionEnum.Iuuok => new DecisionFinderResult(DecisionCode.C07, "IUU OK"),
                ControlAuthorityIuuOptionEnum.IUUNotCompliant => new DecisionFinderResult(DecisionCode.X00, "IUU Not Compliant"),
                ControlAuthorityIuuOptionEnum.Iuuna => new DecisionFinderResult(DecisionCode.C08, "IUU NA"),
                null => new DecisionFinderResult(DecisionCode.X00, "IUU None"),
                _ => new DecisionFinderResult(DecisionCode.E95, "IUU Unknown")
            },
            false => new DecisionFinderResult(DecisionCode.E94, "IUU Not Indicated")
        };
    }
}
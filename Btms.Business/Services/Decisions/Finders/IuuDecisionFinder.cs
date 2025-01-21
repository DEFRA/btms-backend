using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class IuuDecisionFinder : IDecisionFinder
{
    public const string IuuCheckCode = "H224";
    
    public bool CanFindDecision(ImportNotification notification, string[]? checkCodes = null) => notification.ImportNotificationType == ImportNotificationTypeEnum.Cvedp && checkCodes?.Contains(IuuCheckCode) == true;

    public DecisionFinderResult FindDecision(ImportNotification notification, string[]? checkCodes = null)
    {
        return (notification.PartTwo?.ControlAuthority?.IuuCheckRequired == true) switch
        {
            true => notification.PartTwo?.ControlAuthority?.IuuOption switch
            {
                ControlAuthorityIuuOptionEnum.Iuuok => new DecisionFinderResult(DecisionCode.C07, IuuCheckCode, "IUU OK"),
                ControlAuthorityIuuOptionEnum.IUUNotCompliant => new DecisionFinderResult(DecisionCode.X00, IuuCheckCode, "IUU Not Compliant"),
                ControlAuthorityIuuOptionEnum.Iuuna => new DecisionFinderResult(DecisionCode.C08, IuuCheckCode, "IUU NA"),
                null => new DecisionFinderResult(DecisionCode.X00, IuuCheckCode, "IUU None"),
                _ => new DecisionFinderResult(DecisionCode.E95, IuuCheckCode, "IUU Unknown")
            },
            false => new DecisionFinderResult(DecisionCode.E94, IuuCheckCode, "IUU Not Indicated")
        };
    }
}
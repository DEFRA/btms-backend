using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class IuuDecisionFinder : IDecisionFinder
{
    public const string IuuCheckCode = "H224";
    
    public bool CanFindDecision(ImportNotification notification, string? checkCode) => notification.ImportNotificationType == ImportNotificationTypeEnum.Cvedp && checkCode == IuuCheckCode;

    public DecisionFinderResult FindDecision(ImportNotification notification, string? checkCode)
    {
        return (notification.PartTwo?.ControlAuthority?.IuuCheckRequired == true) switch
        {
            true => notification.PartTwo?.ControlAuthority?.IuuOption switch
            {
                ControlAuthorityIuuOptionEnum.Iuuok => new DecisionFinderResult(DecisionCode.C07, checkCode, "IUU OK"),
                ControlAuthorityIuuOptionEnum.IUUNotCompliant => new DecisionFinderResult(DecisionCode.X00, checkCode, "IUU Not Compliant"),
                ControlAuthorityIuuOptionEnum.Iuuna => new DecisionFinderResult(DecisionCode.C08, checkCode, "IUU NA"),
                null => new DecisionFinderResult(DecisionCode.X00, checkCode, "IUU None"),
                _ => new DecisionFinderResult(DecisionCode.E95, checkCode, "IUU Unknown")
            },
            false => new DecisionFinderResult(DecisionCode.E94, checkCode, "IUU Not Indicated")
        };
    }
}
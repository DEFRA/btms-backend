using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class IuuDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification) => notification.PartTwo?.ControlAuthority?.IuuCheckRequired == true;

    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        return notification.PartTwo?.ControlAuthority?.IuuOption switch
        {
            ControlAuthorityIuuOptionEnum.Iuuok => new DecisionFinderResult(DecisionCode.C07, DecisionType.Iuu, "IUU OK"),
            ControlAuthorityIuuOptionEnum.IUUNotCompliant => new DecisionFinderResult(DecisionCode.X00, DecisionType.Iuu, "IUU Not Compliant"),
            ControlAuthorityIuuOptionEnum.Iuuna => new DecisionFinderResult(DecisionCode.C08, DecisionType.Iuu, "IUU NA"),
            _ => new DecisionFinderResult(DecisionCode.X00, DecisionType.Iuu, "IUU None")
        };
    }
}
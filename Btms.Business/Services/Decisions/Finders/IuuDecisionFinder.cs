using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class IuuDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification) => notification.PartTwo?.ControlAuthority?.IuuCheckRequired == true;

    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value);
        }

        return notification.PartTwo?.ControlAuthority?.IuuOption switch
        {
            ControlAuthorityIuuOptionEnum.Iuuok => new DecisionFinderResult(DecisionCode.C07, "TBC"),
            ControlAuthorityIuuOptionEnum.IUUNotCompliant => new DecisionFinderResult(DecisionCode.X00, "TBC"),
            ControlAuthorityIuuOptionEnum.Iuuna => new DecisionFinderResult(DecisionCode.C08, "TBC"),
            _ => new DecisionFinderResult(DecisionCode.X00, "TBC")
        };
    }
}
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class IuuDecisionFinder : IDecisionFinder
{
    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value);
        }

        var controlAuthority = notification.PartTwo?.ControlAuthority;
        return controlAuthority?.IuuCheckRequired switch
        {
            true => controlAuthority?.IuuOption switch
            {
                ControlAuthorityIuuOptionEnum.Iuuok => new DecisionFinderResult(DecisionCode.C07, "TBC"),
                ControlAuthorityIuuOptionEnum.IUUNotCompliant => new DecisionFinderResult(DecisionCode.X00, "TBC"),
                ControlAuthorityIuuOptionEnum.Iuuna => new DecisionFinderResult(DecisionCode.C08, "TBC"),
                _ => new DecisionFinderResult(DecisionCode.X00, "TBC")
            },
            _ => new DecisionFinderResult(DecisionCode.X00)
        };
    }
}
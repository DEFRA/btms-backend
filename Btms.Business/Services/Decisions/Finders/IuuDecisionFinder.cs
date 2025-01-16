using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class IuuDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification) => notification.PartTwo?.ControlAuthority?.IuuCheckRequired == true;

    private static readonly string[]? CheckCodes = ["H224", "C673"];

    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value);
        }

        return notification.PartTwo?.ControlAuthority?.IuuOption switch
        {
            ControlAuthorityIuuOptionEnum.Iuuok => new DecisionFinderResult(DecisionCode.C07, "TBC", CheckCodes),
            ControlAuthorityIuuOptionEnum.IUUNotCompliant => new DecisionFinderResult(DecisionCode.X00, "TBC", CheckCodes),
            ControlAuthorityIuuOptionEnum.Iuuna => new DecisionFinderResult(DecisionCode.C08, "TBC", CheckCodes),
            _ => new DecisionFinderResult(DecisionCode.X00, "TBC", CheckCodes)
        };
    }
}
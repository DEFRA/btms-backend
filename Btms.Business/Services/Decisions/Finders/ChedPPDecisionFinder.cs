using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

// ReSharper disable once InconsistentNaming
public class ChedPPDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification, string[]? _ = null) => notification.ImportNotificationType == ImportNotificationTypeEnum.Chedpp && notification.PartTwo?.ControlAuthority?.IuuCheckRequired != true;

    public DecisionFinderResult FindDecision(ImportNotification notification, string[]? _ = null)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value);
        }

        return new DecisionFinderResult(DecisionCode.E98);
    }
}
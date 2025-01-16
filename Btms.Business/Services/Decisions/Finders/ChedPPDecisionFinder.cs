using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

// ReSharper disable once InconsistentNaming
public class ChedPPDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification) => notification.PartTwo?.ControlAuthority?.IuuCheckRequired == false && notification.ImportNotificationType == ImportNotificationTypeEnum.Chedpp;

    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value);
        }

        return new DecisionFinderResult(DecisionCode.X00);
    }
}
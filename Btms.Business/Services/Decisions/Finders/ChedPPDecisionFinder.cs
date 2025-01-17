using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

// ReSharper disable once InconsistentNaming
public class ChedPPDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification) => notification.ImportNotificationType == ImportNotificationTypeEnum.Chedpp;

    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value, DecisionType.Ched);
        }

        return new DecisionFinderResult(DecisionCode.E98, DecisionType.Ched);
    }
}
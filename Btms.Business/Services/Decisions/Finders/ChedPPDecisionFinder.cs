using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

// ReSharper disable once InconsistentNaming
public class ChedPPDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification, string? checkCode) => notification.ImportNotificationType == ImportNotificationTypeEnum.Chedpp && notification.PartTwo?.ControlAuthority?.IuuCheckRequired != true;

    public DecisionFinderResult FindDecision(ImportNotification notification, string? checkCode)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value, checkCode);
        }

        return new DecisionFinderResult(DecisionCode.E98, checkCode);
    }
}
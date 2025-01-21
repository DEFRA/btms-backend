using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

// ReSharper disable once InconsistentNaming
public class ChedPPDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification, string[]? checkCodes = null) => notification.ImportNotificationType == ImportNotificationTypeEnum.Chedpp && notification.PartTwo?.ControlAuthority?.IuuCheckRequired != true;

    public DecisionFinderResult FindDecision(ImportNotification notification, string[]? checkCodes = null)
    {
        var checkCode = checkCodes?[0];
        
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value, checkCode);
        }

        return new DecisionFinderResult(DecisionCode.E98, checkCode);
    }
}
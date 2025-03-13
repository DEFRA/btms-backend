using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public abstract class DecisionFinder : IDecisionFinder
{
    protected abstract DecisionFinderResult FindDecisionInternal(ImportNotification notification, string? checkCode);

    public abstract bool CanFindDecision(ImportNotification notification, string? checkCode);

    public DecisionFinderResult FindDecision(ImportNotification notification, string? checkCode)
    {
        if (notification.Status == ImportNotificationStatusEnum.Cancelled ||
            notification.Status == ImportNotificationStatusEnum.Replaced)
        {
            return new DecisionFinderResult(DecisionCode.X00, checkCode, InternalDecisionCode: DecisionInternalFurtherDetail.E88);
        }

        return FindDecisionInternal(notification, checkCode);
    }
}
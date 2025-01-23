using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

// ReSharper disable once InconsistentNaming
public class ChedPPDecisionFinder : IDecisionFinder
{
    public static readonly string[] CheckCodes = ["H218", "H220", "H219"];
    public bool CanFindDecision(ImportNotification notification, string? checkCode) => 
        notification.ImportNotificationType == ImportNotificationTypeEnum.Chedpp && CheckCodes.Contains(checkCode);

    public DecisionFinderResult FindDecision(ImportNotification notification, string? checkCode)
    {
        switch (notification.Status)
        {
            case ImportNotificationStatusEnum.Draft:
                break;
            case ImportNotificationStatusEnum.Submitted:
            case ImportNotificationStatusEnum.InProgress:
                return new DecisionFinderResult(DecisionCode.H02, checkCode);
            case ImportNotificationStatusEnum.Validated:
                return new DecisionFinderResult(DecisionCode.C03, checkCode);
            case ImportNotificationStatusEnum.Rejected:
                return new DecisionFinderResult(DecisionCode.N02, checkCode);
            case ImportNotificationStatusEnum.PartiallyRejected:
                return new DecisionFinderResult(DecisionCode.H01, checkCode);
        }

        return new DecisionFinderResult(DecisionCode.E99, checkCode);
    }
}
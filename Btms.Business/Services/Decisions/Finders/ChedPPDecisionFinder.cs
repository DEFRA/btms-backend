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
        return notification.Status switch
        {
            ImportNotificationStatusEnum.Submitted or ImportNotificationStatusEnum.InProgress =>
                new DecisionFinderResult(DecisionCode.H02, checkCode),
            ImportNotificationStatusEnum.Validated => new DecisionFinderResult(DecisionCode.C03, checkCode),
            ImportNotificationStatusEnum.Rejected => new DecisionFinderResult(DecisionCode.N02, checkCode),
            ImportNotificationStatusEnum.PartiallyRejected => new DecisionFinderResult(DecisionCode.H01, checkCode),
            _ => new DecisionFinderResult(DecisionCode.E99, checkCode)
        };
    }
}
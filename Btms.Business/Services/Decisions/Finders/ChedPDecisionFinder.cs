using Btms.Common.Extensions;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedPDecisionFinder : IDecisionFinder
{
    public bool CanFindDecision(ImportNotification notification, string[]? checkCodes = null) => notification.ImportNotificationType == ImportNotificationTypeEnum.Cvedp && checkCodes?.SequenceEqual([IuuDecisionFinder.IuuCheckCode]) != true;

    public DecisionFinderResult FindDecision(ImportNotification notification, string[]? checkCodes = null)
    {
        var checkCode = checkCodes?[0];

        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value, checkCode);
        }

        if (!notification.TryGetConsignmentAcceptable(out var consignmentAcceptable, out var decisionCode))
        {
            return new DecisionFinderResult(decisionCode!.Value, checkCode);
        }
        
        return consignmentAcceptable switch
        {
            true => notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForTranshipment or DecisionDecisionEnum.AcceptableForTransit
                    or DecisionDecisionEnum.AcceptableForSpecificWarehouse =>

                    new DecisionFinderResult(DecisionCode.E03, checkCode),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03, checkCode),
                DecisionDecisionEnum.AcceptableIfChanneled => new DecisionFinderResult(DecisionCode.C06, checkCode),
                _ => new DecisionFinderResult(DecisionCode.E96, checkCode)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02, checkCode),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04, checkCode),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03, checkCode),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07, checkCode),
                _ => new DecisionFinderResult(DecisionCode.E97, checkCode)
            },
            // _ => new DecisionFinderResult(DecisionCode.E99, checkCode)
        };
    }
}
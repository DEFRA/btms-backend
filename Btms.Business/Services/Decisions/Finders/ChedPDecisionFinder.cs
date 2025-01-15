using Btms.Common.Extensions;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public class ChedPDecisionFinder : IDecisionFinder
{
    public DecisionFinderResult FindDecision(ImportNotification notification)
    {
        if (notification.TryGetHoldDecision(out var code))
        {
            return new DecisionFinderResult(code!.Value);
        }

        if (!notification.TryGetConsignmentAcceptable(out var consignmentAcceptable, out var decisionCode))
        {
            return new DecisionFinderResult(decisionCode!.Value);
        }
        
        return consignmentAcceptable switch
        {
            true => notification.PartTwo?.Decision?.DecisionEnum switch
            {
                DecisionDecisionEnum.AcceptableForTranshipment or DecisionDecisionEnum.AcceptableForTransit
                    or DecisionDecisionEnum.AcceptableForSpecificWarehouse =>
                    new DecisionFinderResult(DecisionCode.E03),
                DecisionDecisionEnum.AcceptableForInternalMarket => new DecisionFinderResult(DecisionCode.C03),
                DecisionDecisionEnum.AcceptableIfChanneled => new DecisionFinderResult(DecisionCode.C06),
                _ => new DecisionFinderResult(DecisionCode.E96)
            },
            false => notification.PartTwo?.Decision?.NotAcceptableAction switch
            {
                DecisionNotAcceptableActionEnum.Destruction => new DecisionFinderResult(DecisionCode.N02),
                DecisionNotAcceptableActionEnum.Reexport => new DecisionFinderResult(DecisionCode.N04),
                DecisionNotAcceptableActionEnum.Transformation => new DecisionFinderResult(DecisionCode.N03),
                DecisionNotAcceptableActionEnum.Other => new DecisionFinderResult(DecisionCode.N07),
                _ => new DecisionFinderResult(DecisionCode.E97)
            }
            ,
            // _ => new DecisionFinderResult(DecisionCode.E99)
        };
    }
}
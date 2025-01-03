using Btms.Model;
using Btms.Model.Cds;

namespace Btms.Business.Extensions;

public static class MovementExtensions
{
    public static bool AreNumbersComplete<T>(this IEnumerable<T> source, Func<T, int> getNumbers)
    {
        var numbers = source
            .Select(getNumbers)
            .Order()
            .ToList();

        if (numbers.Distinct().Count() != numbers.Count())
        {
            //Contains duplicates
            return false;
        }
        else if (numbers.Count() != numbers.Last())
        {
            //Some missing
            // should be contiguous

            return false;
        }

        return true;
    }

    public static void AnalyseAlvsStatus(this Movement movement)
    {
        var alvsDecisionStatus = DecisionStatusEnum.InvestigationNeeded;

        if (movement.AlvsDecisionStatus.Decisions.All(d => d.Context.DecisionMatched))
        {
            alvsDecisionStatus = DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs;
        }
        else if (!movement.ClearanceRequests.Exists(c => c.Header!.EntryVersionNumber == 1))
        {
            alvsDecisionStatus = DecisionStatusEnum.AlvsClearanceRequestVersion1NotPresent;
        }
        else if (!movement.ClearanceRequests.AreNumbersComplete(c => c.Header!.EntryVersionNumber!.Value))
        {
            alvsDecisionStatus = DecisionStatusEnum.AlvsClearanceRequestVersionsNotComplete;
        }
        else if (!movement.AlvsDecisionStatus.Decisions.Exists(d => d.Context.AlvsDecisionNumber == 1))
        {
            alvsDecisionStatus = DecisionStatusEnum.AlvsDecisionVersion1NotPresent;
        }
        else if (!movement.AlvsDecisionStatus.Decisions.AreNumbersComplete(d => d.Context.AlvsDecisionNumber!))
        {
            alvsDecisionStatus = DecisionStatusEnum.AlvsDecisionVersionsNotComplete;
        }

        movement.AlvsDecisionStatus.DecisionStatus = alvsDecisionStatus;
    }
    public static void AddLinkStatus(this Movement movement)
    {
        var linkStatus = MovementStatus.NotLinkedStatus;
        var linked = false;
        
        if (movement.Relationships.Notifications.Data.Count > 0)
        {
            linkStatus = MovementStatus.LinkedStatus;
            linked = true;
        }
        else if (movement.AlvsDecisionStatus?.Context?.AlvsCheckStatus?.AnyMatch ?? false)
        {
            linkStatus = MovementStatus.InvestigateStatus;
        }
        
        movement.BtmsStatus.LinkStatus = linkStatus;
        movement.BtmsStatus.Linked = linked;
    }
}
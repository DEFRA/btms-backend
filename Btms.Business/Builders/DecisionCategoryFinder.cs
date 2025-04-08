using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Business.Builders;

public static class DecisionCategoryFinder
{
    private static readonly Dictionary<DecisionCategoryEnum, Func<Movement, bool>> finders = (new()
    {
        { DecisionCategoryEnum.DocumentReferenceFieldIncorrect, DocumentReferenceFieldIncorrect },
        { DecisionCategoryEnum.E89ErrorCode, E89ErrorCode },
        { DecisionCategoryEnum.IpaffsDeletedChed, IpaffsDeletedChed },
        { DecisionCategoryEnum.IpaffsCancelledChed, IpaffsCancelledChed }
    });

    static DecisionCategoryFinder()
    {
        finders = finders.OrderBy(f => f.Key).ToDictionary();

        //Validate that each status in the enum has a finder
        Validate();
    }

    internal static void Validate()
    {
        //Validate that each status in the enum has a finder
        var hasFinders = finders.Select(f => f.Key).ToArray();

        var enumValues = Enum.GetValues<DecisionCategoryEnum>().ToList();

        var missing = enumValues.Except(hasFinders);

        if (missing.Any())
        {
            throw new InvalidOperationException("Decision Status Finders missing in DecisionStatusFinder");
        }
    }

    public static DecisionCategoryEnum? GetDecisionCategory(this Movement movement)
    {
        if (movement.Decisions.Count == 0) return null;

        foreach (var finder in finders)
        {
            if (finder.Value(movement))
            {
                return finder.Key;
            }
        }
        return null;

    }

    private static bool DocumentReferenceFieldIncorrect(Movement movement)
    {
        return movement.Items
            .Any(i => i.Documents?.Any(d => (!MatchIdentifier.IsValid(d.DocumentReference, d.DocumentCode) && !MatchIdentifier.IsIuuRef(d.DocumentReference))) ?? false);
    }

    private static bool IpaffsDeletedChed(Movement movement)
    {

        return movement.AlvsDecisionStatus?
            .Context.ImportNotifications?
            .Any(n => n.Status == ImportNotificationStatusEnum.Deleted) ?? false;

    }

    private static bool IpaffsCancelledChed(Movement movement)
    {
        return movement.AlvsDecisionStatus?
            .Context.ImportNotifications?
            .Any(n => n.Status == ImportNotificationStatusEnum.Cancelled) ?? false;
    }

    private static bool E89ErrorCode(Movement movement)
    {
        return movement.AlvsDecisionStatus?.Decisions.Any(d =>
            d.Decision.Items
                .Any(i => i.Checks
                    .Any(c => c.DecisionInternalFurtherDetail?
                        .Any(difd => difd == "E89") ?? false))) ?? false;
    }
}
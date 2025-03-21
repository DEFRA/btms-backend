using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Business.Builders;

public static class NonComparableDecisionReasonFinder
{
    private static readonly Dictionary<NonComparableDecisionReasonEnum, Func<Movement, bool>> _finders = [];
    private static List<NonComparableDecisionReasonEnum?> _nullableOrderedFinders = [];

    static NonComparableDecisionReasonFinder()
    {
        _finders.Add(NonComparableDecisionReasonEnum.DocumentReferenceFieldIncorrect, DocumentReferenceFieldIncorrect);
        _finders.Add(NonComparableDecisionReasonEnum.E89ErrorCode, E89ErrorCode);
        _finders.Add(NonComparableDecisionReasonEnum.IpaffsDeletedChed, IpaffsDeletedChed);
        _finders.Add(NonComparableDecisionReasonEnum.IpaffsCancelledChed, IpaffsCancelledChed);

        //Validate that each status in the enum has a finder
        Validate();
    }

    internal static void Validate()
    {
        //Validate that each status in the enum has a finder
        var hasFinders = _finders.Select(f => f.Key).ToArray();

        var enumValues = Enum.GetValues<NonComparableDecisionReasonEnum>().ToList();

        var missing = enumValues.Except(hasFinders);

        if (missing.Any())
        {
            throw new InvalidOperationException("Decision Status Finders missing in DecisionStatusFinder");
        }

        // This is so we can get a nullable value from FirstOrDefault below, otherwise it FirstOrDefault returns
        // the first item from the enum as the default.
        _nullableOrderedFinders = enumValues
            .ConvertAll<NonComparableDecisionReasonEnum?>(i => i);
    }

    public static NonComparableDecisionReasonEnum? GetNonComparableDecisionReason(this Movement movement)
    {
        if (movement.Decisions.Count == 0) return null;

        try
        {
            return _nullableOrderedFinders
                .FirstOrDefault(f => _finders[f!.Value!](movement), null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

    }

    private static bool DocumentReferenceFieldIncorrect(Movement movement)
    {
        return movement.Items
            .Any(i => i.Documents?.Any(d => !MatchIdentifier.IsValid(d.DocumentReference)) ?? false);
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
        return false;
    }
}
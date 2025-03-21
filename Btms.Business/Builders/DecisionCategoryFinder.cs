using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Business.Builders;

public static class DecisionCategoryFinder
{
    private static readonly Dictionary<DecisionCategoryEnum, Func<Movement, bool>> _finders = [];
    private static List<DecisionCategoryEnum?> _nullableOrderedFinders = [];

    static DecisionCategoryFinder()
    {
        _finders.Add(DecisionCategoryEnum.DocumentReferenceFieldIncorrect, DocumentReferenceFieldIncorrect);
        _finders.Add(DecisionCategoryEnum.E89ErrorCode, E89ErrorCode);
        _finders.Add(DecisionCategoryEnum.IpaffsDeletedChed, IpaffsDeletedChed);
        _finders.Add(DecisionCategoryEnum.IpaffsCancelledChed, IpaffsCancelledChed);

        //Validate that each status in the enum has a finder
        Validate();
    }

    internal static void Validate()
    {
        //Validate that each status in the enum has a finder
        var hasFinders = _finders.Select(f => f.Key).ToArray();

        var enumValues = Enum.GetValues<DecisionCategoryEnum>().ToList();

        var missing = enumValues.Except(hasFinders);

        if (missing.Any())
        {
            throw new InvalidOperationException("Decision Status Finders missing in DecisionStatusFinder");
        }

        // This is so we can get a nullable value from FirstOrDefault below, otherwise it FirstOrDefault returns
        // the first item from the enum as the default.
        _nullableOrderedFinders = enumValues
            .ConvertAll<DecisionCategoryEnum?>(i => i);
    }

    public static DecisionCategoryEnum? GetDecisionCategory(this Movement movement)
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
            .Any(i => i.Documents?.Any(d => (!MatchIdentifier.IsValid(d.DocumentReference) && !MatchIdentifier.IsIuuRef(d.DocumentReference))) ?? false);
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
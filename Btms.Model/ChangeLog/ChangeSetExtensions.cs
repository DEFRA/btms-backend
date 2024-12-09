namespace Btms.Model.ChangeLog;

public static class ChangeSetExtensions
{
    public static ChangeSet GenerateChangeSet<T>(this T current, T previous)
    {
        return ChangeSet.CreateChangeSet(current, previous);
    }
}
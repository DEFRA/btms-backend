namespace Btms.Common.Extensions;

public static class EnumExtensions
{
    public static bool IsAny<T>(this T e, params T[] args) where T : struct, IComparable
    {
        return args.Any(a => e.Equals(a));
    }
}
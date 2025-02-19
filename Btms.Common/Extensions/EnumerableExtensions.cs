namespace Btms.Common.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source) => source.Where(x => x is not null)!;
}
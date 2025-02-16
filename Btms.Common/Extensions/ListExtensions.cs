namespace Btms.Common.Extensions;

public static class ListExtensions
{
    public static void AddIfNotPresent<T>(this List<T> list, T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
    }

    public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
    {
        foreach (var value in list)
        {
            await func(value);
        }
    }

    public static async IAsyncEnumerable<T> FlattenAsyncEnumerable<T>(this IEnumerable<IAsyncEnumerable<T>> list)
    {
        foreach (var asyncEnumerable in list)
        {
            await foreach (var value in asyncEnumerable)
            {
                yield return value;
            }
        }
    }
}
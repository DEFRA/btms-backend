namespace Btms.Common.Extensions;

public static class ListExtensions
{

    public static T First<T>(this List<T> list)
    {
        return list[0];
    }

    public static T First<T>(this IList<T> list)
    {
        return list[0];
    }

    public static void AddIfNotPresent<T>(this List<T> list, T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
    }

    public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
    {
        await Task.WhenAll(list.Select(func));
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

    public static void Replace<T>(this List<T> list, Predicate<T> oldItemSelector, T newItem)
    {
        //check for different situations here and throw exception
        //if list contains multiple items that match the predicate
        //or check for nullability of list and etc ...
        var oldItemIndex = list.FindIndex(oldItemSelector);
        list[oldItemIndex] = newItem;
    }
}
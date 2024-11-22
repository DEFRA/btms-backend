namespace Cdms.Business.Tests.Commands;

public static class AsyncEnumeratorExtensions
{
    public static TestAsyncEnumerator<T> ToAsyncEnumerator<T>(this IEnumerable<T> items)
    {
        return new TestAsyncEnumerator<T>(items);
    }

    public static TestAsyncEnumerator<T> ToAsyncEnumerator<T>(this T item)
    {
        return new TestAsyncEnumerator<T>([item]);
    }
}
namespace Btms.Business.Tests.Commands;

public class TestAsyncEnumerator<T>(IEnumerable<T> items) : IAsyncEnumerable<T>
{
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        foreach (var item in items)
        {
            yield return await Task.FromResult(item);
        }
    }
}
using Polly;

namespace Btms.Backend.IntegrationTests.Helpers;

public static class ShouldEventually
{
    public static async Task Be(Func<Task> action, int retries = 5, TimeSpan wait = default) =>
        await Policy.Handle<Exception>()
            .WaitAndRetryAsync(retries, _ => wait == TimeSpan.Zero ? TimeSpan.FromMilliseconds(100) : wait)
            .ExecuteAsync(action);

    public static void Be(Action action, int retries = 5, TimeSpan wait = default) =>
        Policy.Handle<Exception>()
            .WaitAndRetry(retries, _ => wait == TimeSpan.Zero ? TimeSpan.FromMilliseconds(100) : wait)
            .Execute(action);
}
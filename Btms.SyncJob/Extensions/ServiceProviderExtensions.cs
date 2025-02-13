using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Btms.SyncJob.Extensions;

public static class ServiceProviderExtensions
{
    public static async Task<bool> WaitOnAllJobs(this IServiceProvider provider, ILogger logger)
    {
        var store = provider.GetService<ISyncJobStore>()!;

        var complete = false;
        while (!complete)
        {
            var jobs = store.GetJobs();
            // logger.LogInformation(jobs.ToJsonString());
            logger.LogInformation("{jobs} found.", jobs.Count);

            complete = true;
        }

        return await Task.FromResult(true);
    }
}
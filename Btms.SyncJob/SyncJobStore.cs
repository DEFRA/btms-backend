using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;

namespace Btms.SyncJob;

public class SyncJobStore(ILogger<SyncJobStore> logger) : ISyncJobStore
{
    private readonly IDictionary<Guid, SyncJob> jobs = new Dictionary<Guid, SyncJob>();
    public SyncJob? GetJob(Guid id)
    {
        return jobs.TryGetValue(id, out var job) ? job : null;
    }
    public List<SyncJob> GetJobs()
    {
        return jobs.Values.ToList();
    }

    public SyncJob CreateJob(Guid id, string? rootFolder, string timespan, string resource)
    {
        var syncJob = new SyncJob(id, rootFolder, timespan, resource);
        jobs[id] = syncJob;
        return syncJob;
    }

    public void ClearSyncJobs(Guid? except = null)
    {
        var itemsToRemove =
            jobs
                .Where(p => !except.HasValue || p.Key != except)
                .ToList();

        itemsToRemove.ForEach(i => jobs.Remove(i));
    }

    public async Task WaitOnJobCompleting(Guid jobId)
    {
        var runningStatues = new List<SyncJobStatus>() { SyncJobStatus.Pending, SyncJobStatus.Running };
        var complete = false;
        while (!complete)
        {
            var job = GetJob(jobId);
            if (job != null)
            {
                logger.LogInformation("JobId {JobId} status {Status}", job.JobId, job.Status);

                if (!runningStatues.Contains(job.Status))
                {
                    complete = true;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
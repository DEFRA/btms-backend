namespace Btms.SyncJob;

public class SyncJobStore : ISyncJobStore
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

    public SyncJob CreateJob(Guid id, string timespan, string resource)
    {
        var syncJob = new SyncJob(id, timespan, resource);
        jobs[id] = syncJob;
        return syncJob;
    }

    public void ClearSyncJobs()
    {
        jobs.Clear();
    }
}
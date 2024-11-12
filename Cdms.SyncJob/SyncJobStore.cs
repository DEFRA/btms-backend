namespace Cdms.SyncJob
{
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

        public SyncJob CreateJob(Guid id, string description)
        {
            var syncJob = new SyncJob(id, description);
            jobs[id] = syncJob;
            return syncJob;
        }
    }
}

namespace Btms.SyncJob;

public interface ISyncJobStore
{
    List<SyncJob> GetJobs();

    SyncJob? GetJob(Guid id);

    SyncJob CreateJob(Guid id, string? rootFolder, string timespan, string resource);

    void ClearSyncJobs(Guid? except = null);

    Task WaitOnJobCompleting(Guid jobId);
}
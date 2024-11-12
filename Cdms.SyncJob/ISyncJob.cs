namespace Cdms.SyncJob;

public interface ISyncJob
{
    Guid JobId { get; }

    string Description { get; }
}
namespace Btms.SyncJob;

public interface ISyncJob
{
    Guid JobId { get; }

    string? RootFolder { get; }

    string Timespan { get; }

    string Resource { get; }
}
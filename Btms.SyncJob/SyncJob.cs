using System.Text.Json.Serialization;

namespace Btms.SyncJob;

public class SyncJob(Guid id, string timespan, string resource) : ISyncJob, IDisposable
{
    private readonly CancellationTokenSource source = new ();
    private bool disposed;
    private int blobsRead;
    private int blobsPublished;
    private int blobsFailed;
    private int messagesProcessed;
    private int messagesFailed;
    private bool readingBlobsFinished;

    public Guid JobId { get; } = id;

    public string Timespan { get; } = timespan;

    public string Resource { get; } = resource;

    public int BlobsRead => blobsRead;

    public int BlobsPublished => blobsPublished;

    public int BlobsFailed => blobsFailed;

    public int MessagesProcessed => messagesProcessed;

    public int MessagesFailed => messagesFailed;

    [JsonIgnore]
    public CancellationToken CancellationToken => source.Token;



    public DateTime QueuedOn { get; } = DateTime.UtcNow;
    public DateTime StartedOn { get; private set; }

    public DateTime? CompletedOn { get; private set; }

    public DateTime? CancelledOn { get; private set; }

    public TimeSpan RunTime
    {
        get
        {
            var endTime = CompletedOn ?? DateTime.UtcNow;
            return endTime.Subtract(StartedOn);
        }
    }

    public SyncJobStatus Status { get; private set; }

    public void BlobSuccess()
    {
        Interlocked.Increment(ref blobsRead);
        Interlocked.Increment(ref blobsPublished);
    }

    public void BlobFailed()
    {
        Interlocked.Increment(ref blobsRead);
        Interlocked.Increment(ref blobsFailed);
    }

    public void MessageFailed()
    {
        Interlocked.Increment(ref messagesFailed);
        MessageProcessed();
    }

    public void MessageProcessed()
    {
        Interlocked.Increment(ref messagesProcessed);
        TryComplete();
    }

    public void CompletedReadingBlobs()
    {
        readingBlobsFinished = true;
        TryComplete();
    }

    public void Start()
    {
        if (Status == SyncJobStatus.Pending)
        {
            Status = SyncJobStatus.Running;
            StartedOn = DateTime.UtcNow;
        }
    }

    public void Complete()
    {
        Status = SyncJobStatus.Completed;
        CompletedOn = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if ((int)Status < 2)
        {
            source.Cancel();
            Status = SyncJobStatus.Cancelled;
            CancelledOn = DateTime.UtcNow;
        }
    }

    private void TryComplete()
    {
        if (readingBlobsFinished && messagesProcessed - blobsPublished == 0)
        {
            Complete();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="CancellationTokenSource" /> class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            source.Dispose();
            disposed = true;
        }
    }
}
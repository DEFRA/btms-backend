namespace Cdms.SyncJob;

public class SyncJob(Guid id, string description) : ISyncJob
{
    private int blobsRead;
    private int blobsPublished;
    private int blobsFailed;
    private int messagesProcessed;
    private int messagesFailed;
    private bool readingBlobsFinished;

    public Guid JobId { get; } = id;

    public string Description { get; } = description;

    public int BlobsRead => blobsRead;

    public int BlobsPublished => blobsPublished;

    public int BlobsFailed => blobsFailed;

    public int MessagesProcessed => messagesProcessed;

    public int MessagesFailed => messagesFailed;



    public DateTime QueuedOn { get; } = DateTime.UtcNow;
    public DateTime StartedOn { get; private set; }

    public DateTime? CompletedOn { get; private set; }

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
        Status = SyncJobStatus.Running;
        StartedOn = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = SyncJobStatus.Completed;
        CompletedOn = DateTime.UtcNow;
    }

    private void TryComplete()
    {
        if (readingBlobsFinished && messagesProcessed - blobsPublished <= 0)
        {
            Complete();
        }
    }
}
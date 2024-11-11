using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Cdms.SyncJob
{
    public enum SyncJobStatus
    {
        Pending,
        Running,
        Completed
    }

    public interface ISyncJob
    {
        Guid JobId { get; }

        string Description { get; }
    }

    public class SyncJobStore : ISyncJobStore
    {
        private IDictionary<Guid, SyncJob> jobs = new Dictionary<Guid, SyncJob>();
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

    public interface ISyncJobStore
    {
        List<SyncJob> GetJobs();

        SyncJob? GetJob(Guid id);

        SyncJob CreateJob(Guid id, string description);
    }

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
            if (readingBlobsFinished)
            {
                if (messagesProcessed - blobsPublished == 0)
                {
                    Complete();
                }
            }
        }
    }

    internal interface ISyncJobQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

        ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
            CancellationToken cancellationToken);
    }

    internal class BackgroundSyncJobQueue : ISyncJobQueue
    {
        private readonly Channel<Func<CancellationToken, ValueTask>> _queue;
        private readonly ILogger<BackgroundSyncJobQueue> _logger;

        public BackgroundSyncJobQueue(ILogger<BackgroundSyncJobQueue> logger)
        {
            var queueCapacity = 10; //refer to docs on setting the capacity
            var options = new BoundedChannelOptions(queueCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);

            _logger = logger;
        }

        public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            _logger.LogInformation("Item {workItemName} has been read and will be dequeued from background queue.", nameof(workItem));

            return workItem;
        }

        public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _logger.LogInformation("Preparing to write item {workItemName} to background queue.", nameof(workItem));

            await _queue.Writer.WriteAsync(workItem);

            _logger.LogInformation("Item {workItemName} has been written to background queue and will be executed.",
                nameof(workItem));
        }
    }
}

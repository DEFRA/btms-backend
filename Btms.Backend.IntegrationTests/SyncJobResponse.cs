using Btms.SyncJob;

namespace Btms.Backend.IntegrationTests
{
    public class SyncJobResponse
    {
        public Guid JobId { get; set; }

        public string Description { get; set; } = null!;

        public int BlobsRead { get; set; }

        public int BlobsPublished { get; set; }

        public int BlobsFailed { get; set; }

        public int MessagesProcessed { get; set; }

        public int MessagesFailed { get; set; }


        public DateTime QueuedOn { get; set; }
        public DateTime StartedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public TimeSpan RunTime { get; set; }

        public SyncJobStatus Status { get; set; }
    }
}
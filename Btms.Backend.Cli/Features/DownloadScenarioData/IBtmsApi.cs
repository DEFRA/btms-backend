using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Business.Commands;
using Btms.SyncJob;
using Refit;

namespace Btms.Backend.Cli.Features.DownloadScenarioData;

public interface IBtmsApi
{
    [Post("/sync/generate-download")]
    Task<string> GenerateDownload([Body] DownloadCommand command);

    [Get("/sync/jobs/{jobId}")]
    Task<SyncJobResponse> GetJobStatus(string jobId);

    [Get("/sync/download/{jobId}")]
    Task<Stream> DownloadFile(string jobId);

    [Get("/analytics/timeline")]
    Task<HistoryResponse> GetMovementTimeline(string movementId);

}

public class HistoryResponse
{
    [JsonInclude]
    public required List<HistoryEntry> Items;
}

public record class HistoryEntry(string ResourceType, string ResourceId);

public static class BtmsApiExtensions
{
    public static async Task WaitOnJobCompleting(this IBtmsApi client, string jobId)
    {
        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        jsonOptions.PropertyNameCaseInsensitive = true;

        var jobStatusTask = Task.Run(async () =>
        {
            var status = SyncJobStatus.Pending;

            while (status != SyncJobStatus.Completed)
            {
                await Task.Delay(200);
                var jobResponse = await client.GetJobStatus(jobId); status = jobResponse.Status;
            }
        });

        var winningTask = await Task.WhenAny(
            jobStatusTask,
            Task.Delay(TimeSpan.FromMinutes(5)));

        if (winningTask != jobStatusTask)
        {
            throw new TimeoutException("Download timeout");
        }
    }
}

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
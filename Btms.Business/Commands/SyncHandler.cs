using Btms.BlobService;
using Btms.SensitiveData;
using Btms.SyncJob;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Btms.Metrics;
using Microsoft.Extensions.Options;
using IRequest = MediatR.IRequest;

namespace Btms.Business.Commands;

public enum SyncPeriod
{
    Today,
    LastMonth,
    ThisMonth,
    All
}

internal static partial class SyncHandlerLogging
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Sync Started for {JobId} - {SyncPeriod} - {Parallelism} - {ProcessorCount} - {Command}")]
    internal static partial void SyncStarted(this ILogger logger, string jobId, string syncPeriod, int parallelism, int processorCount, string command);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing Blob Started {JobId} - {BlobPath}")]
    internal static partial void BlobStarted(this ILogger logger, string jobId, string blobPath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing Blob Finished {JobId} - {BlobPath}")]
    internal static partial void BlobFinished(this ILogger logger, string jobId, string blobPath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing Blob Failed {JobId} - {BlobPath}")]
    internal static partial void BlobFailed(this ILogger logger, Exception exception, string jobId, string blobPath);
}

public abstract class SyncCommand : IRequest, ISyncJob
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; set; }

    public string RootFolder { get; set; } = null!;

    public Guid JobId { get; set; } = Guid.NewGuid();
    public string Timespan => SyncPeriod.ToString();
    public abstract string Resource { get; }
    public string Description => $"{GetType().Name} for {SyncPeriod}";

    internal abstract class Handler<T>(
        SyncMetrics syncMetrics,
        IPublishBus bus,
        ILogger<T> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService,
        IOptions<BusinessOptions> options,
        ISyncJobStore syncJobStore)
        : MediatR.IRequestHandler<T>
        where T : IRequest
    {
        protected readonly BusinessOptions Options = options.Value; 
        
        public const string ActivityName = "Btms.ProcessBlob";

        public abstract Task Handle(T request, CancellationToken cancellationToken);

        protected async Task SyncBlobPaths<TRequest>(SyncPeriod period, string topic, Guid jobId, CancellationToken cancellationToken, params string[] paths)
        {
            var job = syncJobStore.GetJob(jobId);
            job?.Start();
            var degreeOfParallelism = options.Value.GetConcurrency<T>(BusinessOptions.Feature.BlobPaths);
            using (logger.BeginScope(new List<KeyValuePair<string, object>>
                   {
                       new("JobId", job?.JobId!),
                       new("SyncPeriod", period.ToString()),
                       new("Parallelism", degreeOfParallelism),
                       new("ProcessorCount", Environment.ProcessorCount),
                       new("Command", typeof(T).Name),
                   }))
            {
                logger.SyncStarted(job?.JobId.ToString()!, period.ToString(), degreeOfParallelism, Environment.ProcessorCount, typeof(T).Name);
                try
                {
                    await Parallel.ForEachAsync(paths,
                        new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism },
                        async (path, _) =>
                        {
                            using (logger.BeginScope(new List<KeyValuePair<string, object>> { new("SyncPath", path), }))
                            {
                                await SyncBlobPath<TRequest>(path, period, topic, job!, cancellationToken);
                            }
                        });

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error syncing blob paths");
                }
                finally
                {
                    job?.CompletedReadingBlobs();
                    logger.LogInformation("Sync Handler Finished");
                }
            }
        }

        protected async Task SyncBlobPath<TRequest>(string path, SyncPeriod period, string topic, SyncJob.SyncJob job, CancellationToken cancellationToken)
        {
            var result = blobService.GetResourcesAsync($"{path}{period.GetPeriodPath()}", cancellationToken);
            var degreeOfParallelism = options.Value.GetConcurrency<T>(BusinessOptions.Feature.BlobItems);

            await Parallel.ForEachAsync(result, new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = degreeOfParallelism }, async (item, _) =>
            {
                await SyncBlob<TRequest>(path, topic, item, job, cancellationToken);
            });
        }

        protected async Task SyncBlobs<TRequest>(SyncPeriod period, string topic, Guid jobId, CancellationToken cancellationToken, params string[] paths)
        {
            var job = syncJobStore.GetJob(jobId);
            var degreeOfParallelism = options.Value.GetConcurrency<T>(BusinessOptions.Feature.BlobItems);

            job?.Start();
            logger.LogInformation("SyncNotifications period: {Period}, maxDegreeOfParallelism={DegreeOfParallelism}, Environment.ProcessorCount={ProcessorCount}", period.ToString(), degreeOfParallelism, Environment.ProcessorCount);
            try
            {
                foreach (var path in paths)
                {
                    if (job?.Status != SyncJobStatus.Cancelled)
                    {
                        await SyncBlob<TRequest>(path, topic, new BtmsBlobItem { Name = path }, job!, cancellationToken);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing blob paths");
            }
            finally
            {
                job?.CompletedReadingBlobs();
            }
        }

        private async Task SyncBlob<TRequest>(string path, string topic, IBlobItem item, SyncJob.SyncJob job,
            CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();
            using (logger.BeginScope(new List<KeyValuePair<string, object>> { new("BlobPath", item.Name), }))
            {
                try
                {
                    logger.BlobStarted(job.JobId.ToString(), item.Name);
                    syncMetrics.SyncStarted<T>(path, topic);
                    using (var activity = BtmsDiagnostics.ActivitySource.StartActivity(name: ActivityName,
                               kind: ActivityKind.Client, tags: new TagList { { "blob.name", item.Name } }))
                    {
                        var blobContent = await blobService.GetResource(item, cancellationToken);
                        var message = sensitiveDataSerializer.Deserialize<TRequest>(blobContent, _ => { })!;
                        var headers = new Dictionary<string, object>
                        {
                            { "messageId", item.Name.TrimStart(path.ToCharArray()) }, { "jobId", job.JobId }
                        };
                        if (BtmsDiagnostics.ActivitySource.HasListeners())
                        {
                            headers.Add("traceparent", activity?.Id!);
                        }

                        await bus.Publish(message,
                            topic,
                            headers: headers,
                            cancellationToken: cancellationToken);
                    }

                    job.BlobSuccess();
                }
                catch (Exception ex)
                {
                    logger.BlobFailed(ex, job.JobId.ToString(), item.Name);

                    syncMetrics.AddException<T>(ex, path, topic);
                    job.BlobFailed();
                }
                finally
                {
                    syncMetrics.SyncCompleted<T>(path, topic, timer);
                    logger.BlobFinished(job.JobId.ToString(), item.Name);
                }
            }
        }
    }
}
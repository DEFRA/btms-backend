using Cdms.BlobService;
using Cdms.SensitiveData;
using Cdms.SyncJob;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Cdms.Metrics;
using IRequest = MediatR.IRequest;

namespace Cdms.Business.Commands;

public enum SyncPeriod
{
    Today,
    LastMonth,
    ThisMonth,
    All
}

public abstract class SyncCommand() : IRequest, ISyncJob
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; set; }

    public string RootFolder { get; set; } = null!;

    public Guid JobId { get; set; } = Guid.NewGuid();
    public string Timespan => SyncPeriod.ToString();
    public abstract string Resource { get; }
    public string Description => $"{GetType().Name} for {this.SyncPeriod}";

    internal abstract class Handler<T>(
        SyncMetrics syncMetrics,
        IPublishBus bus,
        ILogger<T> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService,
        ISyncJobStore syncJobStore)
        : MediatR.IRequestHandler<T>
        where T : IRequest
    {
        private readonly int maxDegreeOfParallelism = Math.Max(Environment.ProcessorCount / 4, 1);

        public const string ActivityName = "Cdms.ProcessBlob";

        public abstract Task Handle(T request, CancellationToken cancellationToken);

        protected async Task SyncBlobPaths<TRequest>(SyncPeriod period, string topic, Guid jobId, params string[] paths)
        {
            var job = syncJobStore.GetJob(jobId);
            job?.Start();
            using (logger.BeginScope(new List<KeyValuePair<string, object>>
                   {
                       new("JobId", job?.JobId!),
                       new("SyncPeriod", period.ToString()),
                       new("Parallelism", maxDegreeOfParallelism),
                       new("ProcessorCount", Environment.ProcessorCount),
                       new("Command", typeof(T).Name),
                   }))
            {
                logger.LogInformation("Sync Handler Started");
                try
                {
                    await Parallel.ForEachAsync(paths,
                        new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                        async (path, token) =>
                        {
                            using (logger.BeginScope(new List<KeyValuePair<string, object>> { new("SyncPath", path), }))
                            {
                                await SyncBlobPath<TRequest>(path, period, topic, job!, token);
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
                    logger.LogInformation("Sync Handler Started");
                }
            }
        }

        protected async Task SyncBlobPath<TRequest>(string path, SyncPeriod period, string topic, SyncJob.SyncJob job,
            CancellationToken cancellationToken)
        {
            var result = blobService.GetResourcesAsync($"{path}{GetPeriodPath(period)}", cancellationToken);

            await Parallel.ForEachAsync(result, new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = maxDegreeOfParallelism }, async (item, token) =>
            {
                await SyncBlob<TRequest>(path, topic, item, job, token);
            });
        }


        protected async Task SyncBlobs<TRequest>(SyncPeriod period, string topic, Guid jobId, params string[] paths)
        {
            var job = syncJobStore.GetJob(jobId);
            job?.Start();
            logger.LogInformation("SyncNotifications period: {Period}, maxDegreeOfParallelism={MaxDegreeOfParallelism}, Environment.ProcessorCount={ProcessorCount}", period.ToString(), maxDegreeOfParallelism, Environment.ProcessorCount);
            try
            {
                foreach (var path in paths)
                {
                    await SyncBlob<TRequest>(path, topic, new CdmsBlobItem() { Name = path }, job!, CancellationToken.None);
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
                    logger.LogInformation("Processing Blob Started");
                    syncMetrics.SyncStarted<T>(path, topic);
                    using (var activity = CdmsDiagnostics.ActivitySource.StartActivity(name: ActivityName,
                               kind: ActivityKind.Client, tags: new TagList() { { "blob.name", item.Name } }))
                    {
                        var blobContent = await blobService.GetResource(item, cancellationToken);
                        var message = sensitiveDataSerializer.Deserialize<TRequest>(blobContent, _ => { })!;
                        var headers = new Dictionary<string, object>()
                        {
                            { "messageId", item.Name }, { "jobId", job.JobId }
                        };
                        if (CdmsDiagnostics.ActivitySource.HasListeners())
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
                    logger.LogError(ex, "Processing Blob Failed");

                    syncMetrics.AddException<T>(ex, path, topic);
                    job.BlobFailed();
                }
                finally
                {
                    syncMetrics.SyncCompleted<T>(path, topic, timer);
                    logger.LogInformation("Processing Blob Finished");
                }
            }
        }



        private static string GetPeriodPath(SyncPeriod period)
        {
            if (period == SyncPeriod.LastMonth)
            {
                return DateTime.Today.AddMonths(-1).ToString("/yyyy/MM/");
            }
            else if (period == SyncPeriod.ThisMonth)
            {
                return DateTime.Today.ToString("/yyyy/MM/");
            }
            else if (period == SyncPeriod.Today)
            {
                return DateTime.Today.ToString("/yyyy/MM/dd/");
            }
            else if (period == SyncPeriod.All)
            {
                return "/";
            }
            else
            {
                throw new ArgumentException($"Unexpected SyncPeriod {period}");
            }
        }
    }
}
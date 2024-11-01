using System.Diagnostics;
using Cdms.BlobService;
using Cdms.SensitiveData;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using System.Diagnostics.Metrics;
using System.Text.Json.Serialization;
using IRequest = MediatR.IRequest;
using System.Text;
using Cdms.Common;

namespace Cdms.Business.Commands;

public enum SyncPeriod
{
    Today,
    LastMonth,
    ThisMonth,
    All
}

public class SyncMetrics
{
    readonly Histogram<double> syncDuration;
    readonly Counter<long> syncTotal;
    readonly Counter<long> syncFaultTotal;
    readonly Counter<long> syncInProgress;

    public SyncMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Cdms");
        syncTotal = meter.CreateCounter<long>("blob.cdms.sync", "ea", "Number of blobs read");
        syncFaultTotal = meter.CreateCounter<long>("blob.cdms.sync.errors", "ea",
            "Number of sync faults");
        syncInProgress = meter.CreateCounter<long>("blob.cdms.sync.active", "ea",
            "Number of blobs syncing in progress");
        syncDuration = meter.CreateHistogram<double>("blob.cdms.sync.duration", "ms",
            "Elapsed time spent reading the blob, in millis");
    }

    public void AddException(Exception exception, TagList tagList)
    {
        tagList.Add("sync.cdms.exception_type", exception.GetType().Name);
        syncFaultTotal.Add(1, tagList);
    }

    public void SyncStarted(TagList tagList)
    {
        syncTotal.Add(1, tagList);
        syncInProgress.Add(1, tagList);
    }

    public void SyncCompleted(TagList tagList, Stopwatch timer)
    {
        syncInProgress.Add(-1, tagList);
        syncDuration.Record(timer.ElapsedMilliseconds, tagList);
    }
}

public class SyncCommand : IRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; set; }

    internal abstract class Handler<T>(
        SyncMetrics syncMetrics,
        IPublishBus bus,
        ILogger<T> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService)
        : MediatR.IRequestHandler<T>
        where T : IRequest
    {
        public abstract Task Handle(T request, CancellationToken cancellationToken);

        protected async Task<Status> SyncBlobPaths<TRequest>(SyncPeriod period, string topic, params string[] paths)
        {
            logger.LogInformation("SyncNotifications period: {period}", period.ToString());
            try
            {
                var itemCount = 0;
                var erroredCount = 0;

                // TODO need to figure out how we select path

                await Parallel.ForEachAsync(paths, async (path, token) =>
                {
                    var (e, i) = await SyncBlobPath<TRequest>(path, period, topic, token);
                    itemCount += i;
                    erroredCount += e;
                });


                return new Status()
                {
                    Success = true, Description = $"Connected. {itemCount} items upserted. {erroredCount} errors."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());

                return new Status() { Success = false, Description = ex.Message };
            }
        }

        protected async Task<(int, int)> SyncBlobPath<TRequest>(string path, SyncPeriod period, string topic,
            CancellationToken cancellationToken)
        {
            var itemCount = 0;
            var erroredCount = 0;


            // TODO need to figure out how we select path

            var result = blobService.GetResourcesAsync($"{path}{GetPeriodPath(period)}", cancellationToken);

            await Parallel.ForEachAsync(result, cancellationToken, async (item, token) =>
            {
                var success = await SyncBlob<TRequest>(path, topic, item, token);
                if (success)
                {
                    Interlocked.Increment(ref itemCount);
                }
                else
                {
                    Interlocked.Increment(ref erroredCount);
                }
            });


            return (erroredCount, itemCount);
        }

        private async Task<bool> SyncBlob<TRequest>(string path, string topic, IBlobItem item,
            CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();
            var tagList = new TagList
            {
                { "blob.cdms.sync.service", Process.GetCurrentProcess().ProcessName },
                { "blob.cdms.sync.path", path },
                { "blob.cdms.sync.destination", topic },
                { "blob.cdms.sync.message_type", ObservabilityUtils.FormatTypeName(new StringBuilder(), typeof(T)) },
            };
            try
            {
                syncMetrics.SyncStarted(tagList);
                var blobContent = await item.Download(cancellationToken);
                var message = sensitiveDataSerializer.Deserialize<TRequest>(blobContent, _ => { })!;
                await bus.Publish(message,
                    topic,
                    headers: new Dictionary<string, object>() { { "messageId", item.Name } },
                    cancellationToken: cancellationToken);


                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed process blob item {blob}", item.Name);

                syncMetrics.AddException(ex, tagList);

                return false;
            }
            finally
            {
                syncMetrics.SyncCompleted(tagList, timer);
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
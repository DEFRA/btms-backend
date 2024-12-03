using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using Btms.Common;

namespace Btms.Metrics;

public class SyncMetrics
{
    readonly Histogram<double> syncDuration;
    readonly Counter<long> syncTotal;
    readonly Counter<long> syncFaultTotal;
    readonly Counter<long> syncInProgress;

    public SyncMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MetricNames.MeterName);
        syncTotal = meter.CreateCounter<long>("btms.sync", "ea", "Number of blobs read");
        syncFaultTotal = meter.CreateCounter<long>("btms.sync.errors", "ea",
            "Number of sync faults");
        syncInProgress = meter.CreateCounter<long>("btms.sync.active", "ea",
            "Number of blobs syncing in progress");
        syncDuration = meter.CreateHistogram<double>("btms.sync.duration", "ms",
            "Elapsed time spent reading the blob, in millis");
    }

    public void AddException<T>(Exception exception, string path, string destination)
    {
        var tagList = BuildTags<T>(path, destination);
        tagList.Add(MetricNames.CommonTags.ExceptionType, exception.GetType().Name);
       
        syncFaultTotal.Add(1, tagList);
    }

    public void SyncStarted<T>(string path, string destination)
    {
        var tagList = BuildTags<T>(path, destination);

        syncTotal.Add(1, tagList);
        syncInProgress.Add(1, tagList);
    }

    public void SyncCompleted<T>(string path, string destination, Stopwatch timer)
    {
        var tagList = BuildTags<T>(path, destination);

        syncInProgress.Add(-1, tagList);
        syncDuration.Record(timer.ElapsedMilliseconds, tagList);
    }

    private static TagList BuildTags<T>(string path, string destination)
    {
        return new TagList
        {
            { MetricNames.CommonTags.Service, Process.GetCurrentProcess().ProcessName },
            { Constants.Tags.Path, path },
            { MetricNames.CommonTags.QueueName, destination },
            { MetricNames.CommonTags.MessageType, ObservabilityUtils.FormatTypeName(new StringBuilder(), typeof(T)) },
        };
    }

    public static class Constants
    {
        public static class Tags
        {
            public const string Path = "btms.path";
        }
    }
}
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using Btms.Common;

namespace Btms.Metrics;

public class ConsumerMetrics
{
    readonly Histogram<double> consumeDuration;
    readonly Counter<long> consumeTotal;
    readonly Counter<long> consumeFaultTotal;
    readonly Counter<long> consumerInProgress;
    readonly Counter<long> consumeRetryTotal;
    readonly Counter<long> skippedTotal;

    public ConsumerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MetricNames.MeterName);
        consumeTotal = meter.CreateCounter<long>("btms.consume", description: "Number of messages consumed");
        consumeFaultTotal = meter.CreateCounter<long>("btms.consume.errors", description: "Number of message consume faults");
        consumerInProgress = meter.CreateCounter<long>("btms.consume.active", description: "Number of consumers in progress");
        consumeDuration = meter.CreateHistogram<double>("btms.consume.duration", "ms", "Elapsed time spent consuming a message, in millis");
        consumeRetryTotal = meter.CreateCounter<long>("btms.consume.retries", description: "Number of message consume retries");
        skippedTotal = meter.CreateCounter<long>("btms.consume.skipped", description: "Number of message skipped");
    }

    public void Start<TMessage>(string path, string consumerName)
    {
        var tagList = BuildTags<TMessage>(path, consumerName);

        consumeTotal.Add(1, tagList);
        consumerInProgress.Add(1, tagList);
    }

    public void Retry<TMessage>(string path, string consumerName, int attempt)
    {
        var tagList = BuildTags<TMessage>(path, consumerName);

        tagList.Add(Constants.Tags.RetryAttempt, attempt);
        consumeRetryTotal.Add(1, tagList);
    }

    public void Faulted<TMessage>(string path, string consumerName, Exception exception)
    {
        var tagList = BuildTags<TMessage>(path, consumerName);

        tagList.Add(MetricNames.CommonTags.ExceptionType, exception.GetType().Name);
        consumeFaultTotal.Add(1, tagList);
    }

    public void Skipped<TMessage>(string path, string consumerName)
    {
        var tagList = BuildTags<TMessage>(path, consumerName);

        skippedTotal.Add(1, tagList);
    }

    public void Complete<TMessage>(string path, string consumerName, long milliseconds)
    {
        var tagList = BuildTags<TMessage>(path, consumerName);

        consumerInProgress.Add(-1, tagList);
        consumeDuration.Record(milliseconds, tagList);
    }

    private static TagList BuildTags<TMessage>(string path, string consumerName)
    {
        return new TagList
        {
            { MetricNames.CommonTags.Service, Process.GetCurrentProcess().ProcessName },
            { Constants.Tags.Destination, path },
            { MetricNames.CommonTags.MessageType, ObservabilityUtils.FormatTypeName(new StringBuilder(), typeof(TMessage)) },
            { Constants.Tags.ConsumerType, consumerName }
        };
    }

    public static class Constants
    {
        public static class Tags
        {
            public const string Destination = "btms.destination";
            public const string ConsumerType = "btms.consumer_type";
            public const string RetryAttempt = "btms.retry_attempt";
        }
    }
}
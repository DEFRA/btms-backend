using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using Cdms.Common;

namespace Cdms.Consumers.Metrics;

public class ConsumerMetrics
{
    readonly Histogram<double> consumeDuration;
    readonly Counter<long> consumeTotal;
    readonly Counter<long> consumeFaultTotal;
    readonly Counter<long> consumerInProgress;
    readonly Counter<long> consumeRetryTotal;

    public ConsumerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Cdms");
        consumeTotal = meter.CreateCounter<long>("messaging.cdms.consume",  description: "Number of messages consumed");
        consumeFaultTotal = meter.CreateCounter<long>("messaging.cdms.consume.errors", description: "Number of message consume faults");
        consumerInProgress = meter.CreateCounter<long>("messaging.cdms.consume.active", description: "Number of consumers in progress");
        consumeDuration = meter.CreateHistogram<double>("messaging.cdms.consume.duration", "ms", "Elapsed time spent consuming a message, in millis");
        consumeRetryTotal = meter.CreateCounter<long>("messaging.cdms.consume.retries", description: "Number of message consume retries");
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

        tagList.Add("messaging.cdms.retry_attempt", attempt);
        consumeRetryTotal.Add(1, tagList);
    }

    public void Faulted<TMessage>(string path, string consumerName, Exception exception)
    {
        var tagList = BuildTags<TMessage>(path, consumerName);

        tagList.Add("messaging.cdms.exception_type", exception.GetType().Name);
        consumeFaultTotal.Add(1, tagList);
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
            { "messaging.cdms.service", Process.GetCurrentProcess().ProcessName },
            { "messaging.cdms.destination", path },
            { "cdms.message_type", ObservabilityUtils.FormatTypeName(new StringBuilder(), typeof(TMessage)) },
            { "messaging.cdms.consumer_type", consumerName }
        };
    }
}
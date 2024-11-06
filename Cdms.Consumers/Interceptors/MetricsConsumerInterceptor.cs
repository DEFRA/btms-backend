using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Dynamic;
using System.Text;
using System.Text.Json;
using Cdms.Backend.Data;
using Cdms.Common;
using Cdms.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Cdms.Consumers.Interceptors;

public class MetricsConsumerInterceptor<TMessage> : IConsumerInterceptor<TMessage>
{
    readonly Histogram<double> consumeDuration;
    readonly Counter<long> consumeTotal;
    readonly Counter<long> consumeFaultTotal;
    readonly Counter<long> consumerInProgress;
    readonly Counter<long> consumeRetryTotal;

    public MetricsConsumerInterceptor(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Cdms");
        consumeTotal = meter.CreateCounter<long>("messaging.cdms.consume", "ea", "Number of messages consumed");
        consumeFaultTotal = meter.CreateCounter<long>("messaging.cdms.consume.errors", "ea",
            "Number of message consume faults");
        consumerInProgress = meter.CreateCounter<long>("messaging.cdms.consume.active", "ea",
            "Number of consumers in progress");
        consumeDuration = meter.CreateHistogram<double>("messaging.cdms.consume.duration", "ms",
            "Elapsed time spent consuming a message, in millis");
        consumeRetryTotal =
            meter.CreateCounter<long>("messaging.cdms.consume.retries", "ea", "Number of message consume retries");
    }

    public async Task<object> OnHandle(TMessage message, Func<Task<object>> next, IConsumerContext context)
    {
        var timer = Stopwatch.StartNew();
        var tagList = new TagList
        {
            { "messaging.cdms.service", Process.GetCurrentProcess().ProcessName },
            { "messaging.cdms.destination", context.Path },
            {
                "messaging.cdms.message_type",
                ObservabilityUtils.FormatTypeName(new StringBuilder(), typeof(TMessage))
            },
            { "messaging.cdms.consumer_type", context.Consumer.GetType().Name }
        };

        try
        {
            consumeTotal.Add(1, tagList);
            consumerInProgress.Add(1, tagList);
            if (context.Properties.TryGetValue(MessageBusHeaders.RetryCount, out var value))
            {
                tagList.Add("messaging.cdms.retry_attempt", (int)value);
                consumeRetryTotal.Add(1, tagList);
            }

            return await next();
        }
        catch (Exception exception)
        {
            tagList.Add("messaging.cdms.exception_type", exception.GetType().Name);
            consumeFaultTotal.Add(1, tagList);
            throw;
        }
        finally
        {
            consumerInProgress.Add(-1, tagList);
            consumeDuration.Record(timer.ElapsedMilliseconds, tagList);
        }
    }
}
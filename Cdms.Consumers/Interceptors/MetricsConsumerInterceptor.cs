using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Cdms.Consumers.Interceptors;

public class MetricsConsumerInterceptor<TMessage> : IConsumerInterceptor<TMessage>
{
    Histogram<double> consumeDuration;
    Counter<long> consumeTotal;
    Counter<long> consumeFaultTotal;
    Counter<long> consumerInProgress;
    Counter<long> consumeRetryTotal;

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
            { "messaging.cdms.message_type", FormatTypeName(new StringBuilder(), typeof(TMessage)) },
            { "messaging.cdms.consumer_type", context.Consumer.GetType().Name }
        };

        try
        {
            consumeTotal.Add(1, tagList);
            consumerInProgress.Add(1, tagList);
            if (context.Properties.TryGetValue("cdms.retry.count", out var value))
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

    static string FormatTypeName(StringBuilder sb, Type type)
    {
        if (type.IsGenericParameter)
            return "";

        if (type.IsGenericType)
        {
            var name = type.GetGenericTypeDefinition().Name;

            //remove `1
            var index = name.IndexOf('`');
            if (index > 0)
                name = name.Remove(index);

            sb.Append(name);
            sb.Append('_');
            var arguments = type.GenericTypeArguments;
            for (var i = 0; i < arguments.Length; i++)
            {
                if (i > 0)
                    sb.Append('_');

                FormatTypeName(sb, arguments[i]);
            }
        }
        else
            sb.Append(type.Name);

        return sb.ToString();
    }
}
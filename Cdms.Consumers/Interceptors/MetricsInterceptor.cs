using System.Collections.Concurrent;
using System.Diagnostics;
using Cdms.Common;
using Cdms.Common.Extensions;
using Cdms.Consumers.Extensions;
using Cdms.Metrics;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Cdms.Consumers.Interceptors;

public class MetricsInterceptor<TMessage>(InMemoryQueueMetrics queueMetrics, ConsumerMetrics consumerMetrics) : IPublishInterceptor<TMessage>, IConsumerInterceptor<TMessage> where TMessage : notnull
{
    public const string ActivityName = "Cdms.Consumer";
    public const string TimeInQueueActivityName = "Cdms.TimeInQueue";
    private static readonly ConcurrentDictionary<TMessage, DateTime> messageQueueTimes = new();
    public async Task<object> OnHandle(TMessage message, Func<Task<object>> next, IConsumerContext context)
    {
        var timer = Stopwatch.StartNew();
        var deQueueTime = DateTime.UtcNow;

        try
        {
            consumerMetrics.Start<TMessage>(context.Path, context.Consumer.GetType().Name);
            if (context.Properties.TryGetValue(MessageBusHeaders.RetryCount, out var value))
            {
                consumerMetrics.Retry<TMessage>(context.Path, context.Consumer.GetType().Name, (int)value);
                
            }

            var activityContext = context.GetActivityContext();

            if (messageQueueTimes.TryGetValue(message, out var dateTime))
            {
                int msInQueue;
                using (var activity = CdmsDiagnostics.ActivitySource.CreateActivity(TimeInQueueActivityName,
                               ActivityKind.Client, activityContext)
                           ?.SetStartTime(dateTime)
                           .Start())
                {
                    activity?.Stop();
                    activity?.SetEndTime(deQueueTime);
                    msInQueue = deQueueTime.Subtract(dateTime).Milliseconds;
                    queueMetrics.TimeSpentInQueue(msInQueue, context.Path);
                    messageQueueTimes.Remove(message, out var _);

                }
            }

            queueMetrics.Outgoing(queueName: context.Path);
            using (var activity = CdmsDiagnostics.ActivitySource.StartActivity(ActivityName, parentContext: activityContext, kind: ActivityKind.Client))
            {
                var result = await next();
                if (context.WasSkipped())
                {
                    consumerMetrics.Skipped<TMessage>(context.Path, context.Consumer.GetType().Name);
                }
                return result;
            }
        }
        catch (Exception exception)
        {
            consumerMetrics.Faulted<TMessage>(context.Path, context.Consumer.GetType().Name, exception);
            throw;
        }
        finally
        {
            consumerMetrics.Complete<TMessage>(context.Path, context.Consumer.GetType().Name, timer.ElapsedMilliseconds);
            queueMetrics.Completed();
        }
    }

    public Task OnHandle(TMessage message, Func<Task> next, IProducerContext context)
    {
        queueMetrics.Incoming(context.Path);
        messageQueueTimes.TryAdd(message, DateTime.UtcNow);
        return next();
    }
}
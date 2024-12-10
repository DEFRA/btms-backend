using System.Collections.Concurrent;
using System.Diagnostics;
using Btms.Consumers.Extensions;
using Btms.Metrics;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Btms.Consumers.Interceptors;

public class MetricsInterceptor<TMessage>(InMemoryQueueMetrics queueMetrics, ConsumerMetrics consumerMetrics) : IPublishInterceptor<TMessage>, IConsumerInterceptor<TMessage> where TMessage : notnull
{
    public const string ActivityName = "Btms.Consumer";
    public const string TimeInQueueActivityName = "Btms.TimeInQueue";
    private static readonly ConcurrentDictionary<TMessage, DateTime> MessageQueueTimes = new();
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

            if (MessageQueueTimes.TryGetValue(message, out var dateTime))
            {
                int msInQueue;
                using (var activity = BtmsDiagnostics.ActivitySource.CreateActivity(TimeInQueueActivityName,
                               ActivityKind.Client, activityContext)
                           ?.SetStartTime(dateTime)
                           .Start())
                {
                    activity?.Stop();
                    activity?.SetEndTime(deQueueTime);
                    msInQueue = deQueueTime.Subtract(dateTime).Milliseconds;
                    queueMetrics.TimeSpentInQueue(msInQueue, context.Path);
                    MessageQueueTimes.Remove(message, out var _);

                }
            }

            queueMetrics.Outgoing(queueName: context.Path);
            using (BtmsDiagnostics.ActivitySource.StartActivity(ActivityName, parentContext: activityContext, kind: ActivityKind.Client))
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
        MessageQueueTimes.TryAdd(message, DateTime.UtcNow);
        return next();
    }
}
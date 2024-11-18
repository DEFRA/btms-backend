using System.Diagnostics;
using Cdms.Consumers.Metrics;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Cdms.Consumers.Interceptors;

public class MetricsInterceptor<TMessage>(InMemoryQueueMetrics queueMetrics, ConsumerMetrics consumerMetrics) : IPublishInterceptor<TMessage>, IConsumerInterceptor<TMessage> where TMessage : notnull
{
    private static readonly Dictionary<TMessage, DateTime> messageQueueTimes = new();
    public async Task<object> OnHandle(TMessage message, Func<Task<object>> next, IConsumerContext context)
    {
        var timer = Stopwatch.StartNew();

        try
        {
            consumerMetrics.Start<TMessage>(context.Path, context.Consumer.GetType().Name);
            if (context.Properties.TryGetValue(MessageBusHeaders.RetryCount, out var value))
            {
                consumerMetrics.Retry<TMessage>(context.Path, context.Consumer.GetType().Name, (int)value);
                
            }

            if (messageQueueTimes.TryGetValue(message, out var dateTime))
            {
                var msInQueue = DateTime.UtcNow.Subtract(dateTime).Milliseconds;
                queueMetrics.TimeSpentInQueue(msInQueue, context.Path);
                messageQueueTimes.Remove(message);
            }

            queueMetrics.Outgoing(queueName: context.Path);
            return await next();
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
        messageQueueTimes.Add(message, DateTime.UtcNow);
        return next();
    }
}
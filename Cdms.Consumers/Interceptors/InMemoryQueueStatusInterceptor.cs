using Cdms.Consumers.MemoryQueue;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Cdms.Consumers.Interceptors;

public class InMemoryQueueStatusInterceptor<TMessage>(IMemoryQueueStatsMonitor queueStatsMonitor) : IPublishInterceptor<TMessage>, IConsumerInterceptor<TMessage>
{
    public async Task OnHandle(TMessage message, Func<Task> next, IProducerContext context)
    {
        await next();
        queueStatsMonitor.Enqueue(context.Path);
    }

    public async Task<object> OnHandle(TMessage message, Func<Task<object>> next, IConsumerContext context)
    {
        try
        {
            var result = await next();
            return result;
        }
        finally
        {
            queueStatsMonitor.Dequeue(context.Path);
        }
    }
}
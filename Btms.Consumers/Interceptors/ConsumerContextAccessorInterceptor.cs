using Btms.Consumers.ConsumerContextAccessor.ContextAccessor;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Btms.Consumers.Interceptors;

public class ConsumerContextAccessorInterceptor<TMessage>(IConsumerContextAccessor consumerContextAccessor) : IConsumerInterceptor<TMessage>
{
    public async Task<object> OnHandle(TMessage message, Func<Task<object>> next, IConsumerContext context)
    {
        consumerContextAccessor.ConsumerContext = context;
        return await next();
    }
}
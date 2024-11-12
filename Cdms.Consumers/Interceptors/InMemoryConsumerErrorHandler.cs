using Cdms.Consumers.Extensions;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

namespace Cdms.Consumers.Interceptors;

public class InMemoryConsumerErrorHandler<T>(ILogger<InMemoryConsumerErrorHandler<T>> logger)
    : IMemoryConsumerErrorHandler<T>
{
    private async Task<ConsumerErrorHandlerResult> AttemptRetry(IConsumerContext consumerContext,
        Func<Task<object>> retry, Exception exception)
    {
        consumerContext.IncrementRetryAttempt();
        var retryCount = consumerContext.GetRetryAttempt();
        logger.LogError(exception, "Error Consuming Message Retry count {RetryCount}", retryCount);
        if (retryCount > 5)
        {
            return ConsumerErrorHandlerResult.Failure;
        }

        try
        {
            await retry();
        }
        catch (Exception e)
        {
            await AttemptRetry(consumerContext, retry, e);
        }

        return ConsumerErrorHandlerResult.Success;
    }

    public Task<ConsumerErrorHandlerResult> OnHandleError(T message, Func<Task<object>> retry,
        IConsumerContext consumerContext, Exception exception)
    {
        if (!consumerContext.Properties.ContainsKey(MessageBusHeaders.RetryCount))
        {
            consumerContext.Properties.Add(MessageBusHeaders.RetryCount, 0);
        }

        return AttemptRetry(consumerContext, retry, exception);
    }
}
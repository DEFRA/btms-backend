using Btms.Common.Extensions;
using Btms.Consumers.Extensions;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

namespace Btms.Consumers.Interceptors;

public class InMemoryConsumerErrorHandler<T>(ILogger<InMemoryConsumerErrorHandler<T>> logger)
    : IMemoryConsumerErrorHandler<T>
{
    private async Task<ConsumerErrorHandlerResult> AttemptRetry(T message, IConsumerContext consumerContext,
        Func<Task<object>> retry, Exception exception)
    {
        consumerContext.IncrementRetryAttempt();
        var retryCount = consumerContext.GetRetryAttempt();
        
        if (retryCount > 5)
        {
            logger.LogError(exception, "Error Consuming Message Retry count {RetryCount}", retryCount);
            return ConsumerErrorHandlerResult.Failure;
        }

        logger.LogWarning(exception, "Error Consuming Message Retry count {RetryCount}", retryCount);

        try
        {
            await retry();
        }
        catch (Exception e)
        {
            await AttemptRetry(message, consumerContext, retry, e);
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

        return AttemptRetry(message, consumerContext, retry, exception);
    }
}
using Btms.Consumers.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

namespace Btms.Consumers.Interceptors;

public class InMemoryConsumerErrorHandler<T>(ILogger<InMemoryConsumerErrorHandler<T>> logger, IOptions<ConsumerOptions> options) : IMemoryConsumerErrorHandler<T>
{
    private async Task<ProcessResult> AttemptRetry(IConsumerContext consumerContext, Exception exception)
    {
        consumerContext.IncrementRetryAttempt();
        var retryCount = consumerContext.GetRetryAttempt();

        if (retryCount > options.Value.ErrorRetries)
        {
            logger.LogError(exception, "Error Consuming Message Retry count {RetryCount}", retryCount);
            return ProcessResult.Failure;
        }

        logger.LogWarning(exception, "Error Consuming Message Retry count {RetryCount}", retryCount);

        try
        {
            await Task.Delay(retryCount * 2000);
            return ProcessResult.Retry;
        }
        catch (Exception e)
        {
            await AttemptRetry(consumerContext, e);
        }

        return ProcessResult.Success;
    }

    public Task<ProcessResult> OnHandleError(T message, IConsumerContext consumerContext, Exception exception, int attempts)
    {
        if (!consumerContext.Properties.ContainsKey(MessageBusHeaders.RetryCount))
        {
            consumerContext.Properties.Add(MessageBusHeaders.RetryCount, 0);
        }

        return AttemptRetry(consumerContext, exception);
    }
}
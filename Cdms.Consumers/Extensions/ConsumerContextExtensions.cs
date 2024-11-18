using SlimMessageBus;

namespace Cdms.Consumers.Extensions;

public static class ConsumerContextExtensions
{
    public static int GetRetryAttempt(this IConsumerContext consumerContext)
    {
        if (consumerContext.Properties.TryGetValue(MessageBusHeaders.RetryCount, out var value))
        {
            var retryCount = (int)value;
            return retryCount;
        }

        return 0;
    }

    public static void IncrementRetryAttempt(this IConsumerContext consumerContext)
    {
        var retryCount = consumerContext.GetRetryAttempt();
        retryCount++;
        consumerContext.Properties[MessageBusHeaders.RetryCount] = retryCount;
    }
}
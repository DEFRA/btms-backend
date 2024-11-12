using SlimMessageBus;

namespace Cdms.Consumers.Extensions;

public static class ConsumerContextExtensions
{
    public static int GetRetryAttempt(this IConsumerContext consumerContext)
    {
        var value = consumerContext.Properties[MessageBusHeaders.RetryCount];
        if (value is not null)
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
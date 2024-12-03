using SlimMessageBus;
using System.Diagnostics;

namespace Btms.Consumers.Extensions;

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

    public static string? GetJobId(this IConsumerContext consumerContext)
    {
        if (consumerContext.Headers.TryGetValue(MessageBusHeaders.JobId, out var value))
        {
            return value?.ToString();
        }

        return null;
    }

    public static ActivityContext GetActivityContext(this IConsumerContext consumerContext)
    {
        if (consumerContext.Properties.TryGetValue(MessageBusHeaders.TraceParent, out var value))
        {
            return ActivityContext.Parse(value.ToString()!, null);
        }

        return new ActivityContext();
    }

    public static void Skipped(this IConsumerContext consumerContext)
    {
        consumerContext.Properties.Add(MessageBusHeaders.Skipped, true);
    }

    public static bool WasSkipped(this IConsumerContext consumerContext)
    {
        if (consumerContext.Properties.TryGetValue(MessageBusHeaders.Skipped, out _))
        {
            return true;
        }

        return false;
    }

    public static void IncrementRetryAttempt(this IConsumerContext consumerContext)
    {
        var retryCount = consumerContext.GetRetryAttempt();
        retryCount++;
        consumerContext.Properties[MessageBusHeaders.RetryCount] = retryCount;
    }
}
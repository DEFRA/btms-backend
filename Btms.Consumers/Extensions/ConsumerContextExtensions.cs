using SlimMessageBus;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;

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

    public static string GetMessageId(this IConsumerContext consumerContext)
    {
        if (consumerContext.Headers.TryGetValue(MessageBusHeaders.MessageId, out var value))
        {
            return value.ToString()!;
        }

        if (consumerContext.Properties.TryGetValue(MessageBusHeaders.ServiceBusMessage, out var sbMessage))
        {
            return ((ServiceBusReceivedMessage)sbMessage).MessageId;
        }

        return string.Empty;
    }

    public static string GetMessageType(this IConsumerContext consumerContext)
    {
        if (consumerContext.Headers.TryGetValue(MessageBusHeaders.MessageType, out var value))
        {
            return value.ToString()!;
        }

        return string.Empty;
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
        consumerContext.Properties.TryAdd(MessageBusHeaders.Skipped, true);
    }

    public static void PreProcessed(this IConsumerContext consumerContext)
    {
        consumerContext.Properties.TryAdd(MessageBusHeaders.PreProcessed, true);
    }

    public static bool IsPreProcessed(this IConsumerContext consumerContext)
    {
        if (consumerContext.Properties.TryGetValue(MessageBusHeaders.PreProcessed, out var value))
        {
            return (bool)value;
        }

        return false;
    }

    public static void Linked(this IConsumerContext consumerContext)
    {
        consumerContext.Properties.TryAdd(MessageBusHeaders.Linked, true);
    }

    public static bool IsLinked(this IConsumerContext consumerContext)
    {
        if (consumerContext.Properties.TryGetValue(MessageBusHeaders.Linked, out var value))
        {
            return (bool)value;
        }

        return false;
    }

    public static bool UseLock(this IConsumerContext consumerContext)
    {
        if (consumerContext.Headers.TryGetValue(MessageBusHeaders.UseLock, out var value))
        {
            return (bool)value;
        }

        return false;
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
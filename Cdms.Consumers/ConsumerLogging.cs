using Microsoft.Extensions.Logging;

namespace Cdms.Consumers;

internal static partial class ConsumerLogging
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Message skipped - {JobId} - {MessageId} - {Consumer} - {Identifier}")]
    internal static partial void ConsumerStarted(this ILogger logger, string jobId, string messageId, string consumer, string identifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "Message skipped - {JobId} - {MessageId} - {Consumer} - {Identifier}")]
    internal static partial void MessageSkipped(this ILogger logger, string jobId, string messageId, string consumer, string identifier);
}
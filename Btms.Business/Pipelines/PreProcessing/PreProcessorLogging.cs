using Microsoft.Extensions.Logging;

namespace Btms.Business.Pipelines.PreProcessing;

internal static partial class PreProcessorLogging
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Message skipped - {MessageId} - {Identifier}")]
    internal static partial void MessageSkipped(this ILogger logger, string messageId, string identifier);
}
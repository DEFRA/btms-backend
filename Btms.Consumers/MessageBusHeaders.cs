namespace Btms.Consumers;

public static class MessageBusHeaders
{
    public const string RetryCount = "btms.retry.count";
    public const string TraceParent = "traceparent";
    public const string JobId = "jobId";
    public const string MessageId = "messageId";
    public const string Skipped = "skipped";
    public const string PreProcessed = "pre-processed";
    public const string Linked = "linked";
}
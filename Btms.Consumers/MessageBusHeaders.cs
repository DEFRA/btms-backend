namespace Btms.Consumers;

public static class MessageBusHeaders
{
    public const string RetryCount = "btms.retry.count";
    public const string TraceParent = "traceparent";
    public const string JobId = "jobId";
    public const string Skipped = "skipped";
}
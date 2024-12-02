namespace Cdms.Consumers;

public static class MessageBusHeaders
{
    public const string RetryCount = "cdms.retry.count";
    public const string TraceParent = "traceparent";
    public const string JobId = "jobId";
    public const string Skipped = "skipped";
}
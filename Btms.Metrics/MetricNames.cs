namespace Btms.Metrics;

public static class MetricNames
{
    public const string MeterName = "Btms.Backend";

    public static class CommonTags
    {
        public const string Service = "btms.service";
        public const string ExceptionType = "btms.exception_type";
        public const string MessageType = "btms.message_type";
        public const string QueueName = "btms.memory.queue_name";
    }
}
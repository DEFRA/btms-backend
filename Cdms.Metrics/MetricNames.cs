namespace Cdms.Metrics
{
    public static class MetricNames
    {
        public const string MeterName = "Cdms.Backend";

        public static class CommonTags
        {
            public const string Service = "cdms.service";
            public const string ExceptionType = "cdms.exception_type";
            public const string MessageType = "cdms.message_type";
            public const string QueueName = "cdms.memory.queue_name";
        }
    }
}

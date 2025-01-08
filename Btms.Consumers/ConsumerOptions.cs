namespace Btms.Consumers;

public class ConsumerOptions
{
    public const string SectionName = nameof(ConsumerOptions);

    public bool EnableBlockingPublish { get; set; } = false;
    public int InMemoryNotifications { get; set; } = 20;
    public int InMemoryGmrs { get; set; } = 2;
    public int InMemoryClearanceRequests { get; set; } = 20;
    public int InMemoryDecisions { get; set; } = 2;

    public int ErrorRetries { get; set; } = 10;

}
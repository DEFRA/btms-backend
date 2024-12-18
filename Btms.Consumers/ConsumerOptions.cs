namespace Btms.Consumers;

public class ConsumerOptions
{
    public const string SectionName = nameof(ConsumerOptions);

    public bool EnableBlockingPublish { get; set; } = false;
    public int InMemoryNotifications { get; set; } = 2;
    public int InMemoryGmrs { get; set; } = 2;
    public int InMemoryClearanceRequests { get; set; } = 2;
    public int InMemoryDecisions { get; set; } = 2;
    
}
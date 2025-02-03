namespace Btms.Consumers;

public class ConsumerOptions
{
    public const string SectionName = nameof(ConsumerOptions);

    public bool EnableBlockingPublish { get; set; } = false;
    
    public int InMemoryNotifications { get; set; } = 20;
    public int InMemoryGmrs { get; set; } = 20;
    public int InMemoryClearanceRequests { get; set; } = 20;
    public int InMemoryDecisions { get; set; } = 20;
    public int InMemoryFinalisations { get; set; } = 20;
    
    public int AsbAlvsMessages { get; set; } = 20;
    public int AsbNotifications { get; set; } = 20;

    public int ErrorRetries { get; set; } = 10;

    public bool EnableAsbConsumers { get; set; }

}
using System.ComponentModel.DataAnnotations;

namespace Btms.Consumers;

public class ServiceBusOptions
{
    public static readonly string SectionName = nameof(ServiceBusOptions);
    
    [Required]
    public required Dictionary<string, ServiceBusSubscriptionOptions> Subscriptions { get; set; }

    public ServiceBusSubscriptionOptions AlvsSubscription => Subscriptions["alvs"];

    public ServiceBusSubscriptionOptions NotificationSubscription => Subscriptions["notification"];

    public ServiceBusSubscriptionOptions GmrSubscription => Subscriptions["gmr"];
}

public class ServiceBusSubscriptionOptions
{
    [Required]
    public required string ConnectionString { get; set; }

    [Required]
    public required string Topic { get; set; }

    [Required]
    public required string Subscription { get; set; }
}
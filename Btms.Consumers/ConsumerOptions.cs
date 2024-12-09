using System.ComponentModel.DataAnnotations;
using Btms.Azure;

namespace Btms.Consumers;

public class ConsumerOptions
{
    public const string SectionName = nameof(ConsumerOptions);

    public int InMemoryNotifications { get; set; } = 2;
    public int InMemoryGmrs { get; set; } = 2;
    public int InMemoryClearanceRequests { get; set; } = 2;
    public int InMemoryDecisions { get; set; } = 2;
    
}
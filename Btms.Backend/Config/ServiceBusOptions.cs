using System.ComponentModel.DataAnnotations;

namespace Btms.Backend.Config;

public class ServiceBusOptions
{
    public static readonly string SectionName = nameof(ServiceBusOptions);

    [Required]
    public required string ConnectionString { get; set; }

    [Required]
    public required string Topic { get; set; }

    [Required]
    public required string Subscription { get; set; }
}
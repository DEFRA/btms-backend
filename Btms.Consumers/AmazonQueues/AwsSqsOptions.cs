using System.ComponentModel.DataAnnotations;

namespace Btms.Consumers.AmazonQueues;

public class AwsSqsOptions
{
    public const string SectionName = nameof(AwsSqsOptions);

    public string? Region { get; set; }

    public string? ServiceUrl { get; set; }

    public string? AccessKeyId { get; set; }

    public string? SecretAccessKey { get; set; }

    [Required]
    public required string ClearanceRequestQueueName { get; set; }

    [Required]
    public required string DecisionQueueName { get; set; }

    [Required]
    public required string FinalisationQueueName { get; set; }
}
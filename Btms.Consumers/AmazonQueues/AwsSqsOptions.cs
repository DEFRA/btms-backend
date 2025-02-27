using System.ComponentModel.DataAnnotations;

namespace Btms.Consumers.AmazonQueues;

public class AwsSqsOptions
{
    public const string SectionName = nameof(AwsSqsOptions);

    [Required]
    public required string Region { get; set; }

    [Required]
    public required string ServiceUrl { get; set; }

    [Required]
    public required string AccessKeyId { get; set; }

    [Required]
    public required string SecretAccessKey { get; set; }

    [Required]
    public required string ClearanceRequestQueueName { get; set; }
}
namespace Btms.Consumers.AmazonQueues;

public class AwsLocalOptions
{
    public const string SectionName = "AwsOptions";
    
    public string? Region { get; init; }
    public string? ServiceUrl { get; init; }
    public string? AccessKeyId { get; init; }
    public string? SecretAccessKey { get; init; }
}
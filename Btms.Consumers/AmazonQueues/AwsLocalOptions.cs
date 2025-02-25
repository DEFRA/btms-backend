using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Btms.Consumers.AmazonQueues;

public class AwsLocalOptions(IConfiguration configuration)
{
    public string? Region => configuration["AWS_DEFAULT_REGION"];
    public string? ServiceUrl => configuration["AWS_ENDPOINT_URL"];
    public string? AccessKeyId => configuration["AWS_ACCESS_KEY_ID"];
    public string? SecretAccessKey => configuration["AWS_SECRET_ACCESS_KEY"];

    [SuppressMessage("SonarLint", "S5332", Justification = "The URL is a local one so none secure HTTP is fine")]
    public static Dictionary<string, string?> DefaultLocalConfig { get; private set; } = new()
    {
        { "AWS_DEFAULT_REGION", "eu-west-2" },
        { "AWS_ENDPOINT_URL", "http://sqs.eu-west-2.localhost.localstack.cloud:4566" },
        { "AWS_ACCESS_KEY_ID", "local" },
        { "AWS_SECRET_ACCESS_KEY", "local" }
    };
}
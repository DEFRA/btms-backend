using System.Diagnostics.CodeAnalysis;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public static class AwsConfig
{
    [SuppressMessage("SonarLint", "S5332", Justification = "The URL is a local one so none secure HTTP is fine")]
    public static Dictionary<string, string?> DefaultLocalConfig { get; private set; } = new()
    {
        { "AWS_DEFAULT_REGION", "eu-west-2" },
        { "AWS_ENDPOINT_URL", "http://sqs.eu-west-2.localhost.localstack.cloud:4966" },
        { "AWS_ACCESS_KEY_ID", "local" },
        { "AWS_SECRET_ACCESS_KEY", "local" }
    };
}
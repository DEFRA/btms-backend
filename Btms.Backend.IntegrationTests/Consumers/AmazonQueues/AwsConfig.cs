using System.Diagnostics.CodeAnalysis;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public static class AwsConfig
{
    [SuppressMessage("SonarLint", "S5332", Justification = "The URL is a local one so none secure HTTP is fine")]
    public static Dictionary<string, string?> DefaultLocalConfig { get; private set; } = new()
    {
        { "AwsSqsOptions:Region", "eu-west-2" },
        { "AwsSqsOptions:ServiceUrl", "http://sqs.eu-west-2.localhost.localstack.cloud:4966" },
        { "AwsSqsOptions:AccessKeyId", "local" },
        { "AwsSqsOptions:SecretAccessKey", "local" }
    };
}
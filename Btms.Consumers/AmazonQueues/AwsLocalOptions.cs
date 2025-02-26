using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Btms.Consumers.AmazonQueues;

public class AwsLocalOptions(IConfiguration configuration)
{
    public string? Region => configuration["AWS_DEFAULT_REGION"];
    public string? ServiceUrl => configuration["AWS_ENDPOINT_URL"];
    public string? AccessKeyId => configuration["AWS_ACCESS_KEY_ID"];
    public string? SecretAccessKey => configuration["AWS_SECRET_ACCESS_KEY"];
}
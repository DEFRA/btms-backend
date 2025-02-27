using Btms.Consumers.AmazonQueues;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Btms.Backend.Aws;

public static class BtmsSqsHealthCheckBuilderExtensions
{
    private const string NAME = "aws sqs";

    public static IHealthChecksBuilder AddBtmsSqs(
        this IHealthChecksBuilder builder,
        AwsSqsOptions options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new BtmsSqsHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
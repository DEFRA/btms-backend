using Btms.Common.Extensions;
using Btms.Consumers.AmazonQueues;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Btms.Backend.Aws;

public static class BtmsSqsHealthCheckBuilderExtensions
{
    private const string Name = "aws sqs";

    public static IHealthChecksBuilder AddBtmsSqs(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        builder.Services.BtmsAddOptions<AwsSqsOptions>(configuration, AwsSqsOptions.SectionName).Get();

        return builder;
    }
}
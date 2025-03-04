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
        var awsOptions = builder.Services.BtmsAddOptions<AwsSqsOptions>(configuration, AwsSqsOptions.SectionName).Get();

        builder.Add(new HealthCheckRegistration(
            $"{Name} clearance requests",
            _ => new BtmsSqsHealthCheck(awsOptions, awsOptions.ClearanceRequestQueueName),
            failureStatus,
            tags,
            timeout));

        builder.Add(new HealthCheckRegistration(
            $"{Name} decisions",
            _ => new BtmsSqsHealthCheck(awsOptions, awsOptions.DecisionQueueName),
            failureStatus,
            tags,
            timeout));

        return builder;
    }
}
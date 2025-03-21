using Btms.Common.Extensions;
using Btms.Common.FeatureFlags;
using Btms.Consumers.AmazonQueues;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

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
            sp => BuildHealthCheck(sp, Features.HealthChecks.Sqs.ClearanceRequests, awsOptions.ClearanceRequestQueueName),
            failureStatus,
            tags,
            timeout));

        builder.Add(new HealthCheckRegistration(
            $"{Name} decisions",
            sp => BuildHealthCheck(sp, Features.HealthChecks.Sqs.Decisions, awsOptions.DecisionQueueName),
            failureStatus,
            tags,
            timeout));

        builder.Add(new HealthCheckRegistration(
            $"{Name} finalisations",
            sp => BuildHealthCheck(sp, Features.HealthChecks.Sqs.Finalisations, awsOptions.FinalisationQueueName),
            failureStatus,
            tags,
            timeout));

        return builder;
    }

    private static IHealthCheck BuildHealthCheck(IServiceProvider sp, string feature, string queueName)
    {
        var awsOptions = sp.GetRequiredService<IOptions<AwsSqsOptions>>();
        return new FeatureFlagHealthCheck(sp.GetRequiredService<IFeatureManager>(), feature,
            new BtmsSqsHealthCheck(awsOptions.Value, queueName));
    }
}
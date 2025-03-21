using Amazon.SQS;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Amazon;
using Amazon.Runtime;
using Btms.Consumers.AmazonQueues;

namespace Btms.Backend.Aws;

public class BtmsSqsHealthCheck(AwsSqsOptions sqsOptions, string queueName) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSqsClient();
            _ = await client.GetQueueUrlAsync(queueName, cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private IAmazonSQS CreateSqsClient()
    {
        if (sqsOptions.ServiceUrl != null)
        {
            return new AmazonSQSClient(new BasicAWSCredentials(sqsOptions.AccessKeyId, sqsOptions.SecretAccessKey), new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(sqsOptions.Region),
                ServiceURL = sqsOptions.ServiceUrl
            });
        }

        return new AmazonSQSClient();
    }
}
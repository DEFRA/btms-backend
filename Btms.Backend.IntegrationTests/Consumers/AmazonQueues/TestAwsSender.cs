using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Btms.Consumers.AmazonQueues;
using Btms.Types.Alvs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public class TestAwsSender
{
    private readonly IAmazonSimpleNotificationService _snsSender;
    private string _topicArnPrefix;

    public TestAwsSender(IConfiguration configuration)
    {
        var awsLocalOptions = configuration
            .GetSection(AwsLocalOptions.SectionName)
            .Get<AwsLocalOptions>();

        var serviceCollection = new ServiceCollection();

        var awsOptions = configuration.GetAWSOptions();
        if (awsLocalOptions?.ServiceUrl != null)
        {
            awsOptions.Credentials = new BasicAWSCredentials(awsLocalOptions.AccessKeyId, awsLocalOptions.SecretAccessKey);
            awsOptions.DefaultClientConfig.ServiceURL = awsLocalOptions.ServiceUrl;
            awsOptions.Region = RegionEndpoint.GetBySystemName(awsLocalOptions.Region);
            awsOptions.DefaultClientConfig.AuthenticationRegion = awsLocalOptions.Region;
        }

        serviceCollection.AddDefaultAWSOptions(awsOptions);
        serviceCollection.AddAWSService<IAmazonSimpleNotificationService>();

        var services = serviceCollection.BuildServiceProvider();

        _snsSender = services.GetRequiredService<IAmazonSimpleNotificationService>();

        var response = _snsSender.ListTopicsAsync().Result;
        var topicArn = response.Topics.First().TopicArn;
        _topicArnPrefix = topicArn[..topicArn.LastIndexOf(':')];
    }

    public async Task SendAsync<T>(T message) where T : class
    {
        var topicName = message switch
        {
            AlvsClearanceRequest => "customs_clearance_request",
            _ => throw new ArgumentException("Invalid message type", nameof(message))
        };

        await _snsSender.PublishAsync(new PublishRequest
        {
            TopicArn = $"{_topicArnPrefix}:{topicName}.fifo",
            Message = JsonSerializer.Serialize(message),
            MessageDeduplicationId = Guid.NewGuid().ToString(),
            MessageGroupId = "message-group-id"
        });
    }
}
using System.Text.Json;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Btms.Consumers.AmazonQueues;
using Btms.Types.Alvs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public class TestAwsSender
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IAmazonSimpleNotificationService _snsSender;
    private readonly string _topicArnPrefix;

    public TestAwsSender(IConfiguration configuration, AwsLocalOptions awsLocalOptions, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var serviceCollection = new ServiceCollection();

        var awsOptions = configuration.GetAWSOptions();

        testOutputHelper.WriteLine($"Configure AWS Test Sender: ServiceURL={awsLocalOptions.ServiceUrl}");

        if (awsLocalOptions.ServiceUrl != null)
        {
            awsOptions.DefaultClientConfig.ServiceURL = awsLocalOptions.ServiceUrl;
            awsOptions.Credentials = new BasicAWSCredentials(awsLocalOptions.AccessKeyId, awsLocalOptions.SecretAccessKey);
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

        var publishRequest = new PublishRequest
        {
            TopicArn = $"{_topicArnPrefix}:{topicName}.fifo",
            Message = JsonSerializer.Serialize(message),
            MessageDeduplicationId = Guid.NewGuid().ToString(),
            MessageGroupId = "message-group-id"
        };

        _testOutputHelper.WriteLine($"Publish message body to {publishRequest.TopicArn} of type {typeof(T).Name} with message: {publishRequest.Message}");

        await _snsSender.PublishAsync(publishRequest);
    }
}
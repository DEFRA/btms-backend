using System.Text.Json;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Btms.Consumers.AmazonQueues;
using Btms.Types.Alvs;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public class TestAwsSender : IAsyncDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IAmazonSimpleNotificationService _snsSender;
    private readonly string _topicArnPrefix = null!;
    private readonly ServiceProvider _services;
    
    public readonly List<Topic>? Topics;
    public readonly List<Subscription>? Subscriptions;

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

        _services = serviceCollection.BuildServiceProvider();

        _snsSender = _services.GetRequiredService<IAmazonSimpleNotificationService>();

        for (var i = 0; i < 5; i++)
        {
            Topics = _snsSender.ListTopicsAsync().Result.Topics;
            Subscriptions = _snsSender.ListSubscriptionsAsync().Result.Subscriptions;
            var topicArn = Topics.FirstOrDefault()?.TopicArn;
            if (topicArn == null)
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                continue;
            }
            _topicArnPrefix = topicArn[..topicArn.LastIndexOf(':')];
        }

        _topicArnPrefix.Should().NotBeNull();
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

    public async ValueTask DisposeAsync()
    {
        _snsSender.Dispose();
        await _services.DisposeAsync();
    }
}
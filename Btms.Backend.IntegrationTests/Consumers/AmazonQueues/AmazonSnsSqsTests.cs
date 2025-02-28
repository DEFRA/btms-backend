using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

[Collection("AwsSqsSns")]
[Trait("Category", "Integration")]
public class AmazonSnsSqsTests : IAsyncLifetime
{
    private readonly TestAwsConsumers _awsConsumers = new();
    private readonly TestAwsSender _awsSender;
    private readonly ITestOutputHelper _testOutputHelper;

    public AmazonSnsSqsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _awsSender = new TestAwsSender(_awsConsumers.Configuration, _awsConsumers.AwsSqsOptions, _testOutputHelper);
    }

    [Fact]
    public async Task WhenCheckingIfLocalstackAvailableThenShouldBeAvailableInDevelopment()
    {
        var httpClient = new HttpClient();

        var response = await httpClient.GetAsync($"{_awsConsumers.AwsSqsOptions.ServiceUrl?.TrimEnd('/')}/_localstack/health");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public void WhenCheckingIfAwsSnsAvailableThenShouldBeAbleToEnumerateSndTopics()
    {
        _testOutputHelper.WriteLine("SNS Topics found:");
        foreach (var topic in _awsSender.Topics ?? [])
        {
            _testOutputHelper.WriteLine($" - {topic.TopicArn}");
        }

        _awsSender.Topics.Should().HaveCount(5);
    }

    [Fact]
    public void WhenCheckingIfAwsSnsSqsAvailableThenShouldBeAbleToEnumerateSnsSqsSubscriptions()
    {
        _testOutputHelper.WriteLine("SNS Subscriptions found:");
        foreach (var subscription in _awsSender.Subscriptions ?? [])
        {
            _testOutputHelper.WriteLine($" - {subscription.TopicArn} - {subscription.Endpoint} - {subscription.SubscriptionArn}");
        }

        _awsSender.Subscriptions.Should().HaveCount(5);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _awsSender.DisposeAsync();
        await _awsConsumers.DisposeAsync();
    }
}
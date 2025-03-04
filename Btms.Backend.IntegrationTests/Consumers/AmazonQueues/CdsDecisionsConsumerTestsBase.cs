using Btms.Types.Alvs;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

[Collection("AwsSqsSns")]
[Trait("Category", "Integration")]
public class CdsDecisionsConsumerTestsBase : IAsyncLifetime
{
    private readonly TestAwsConsumers _awsConsumers = new();
    private readonly TestAwsSender _awsSender;

    public CdsDecisionsConsumerTestsBase(ITestOutputHelper testOutputHelper)
    {
        _awsSender = new TestAwsSender(_awsConsumers.Configuration, _awsConsumers.AwsSqsOptions, testOutputHelper);
    }

    [Fact]
    public async Task When_receiving_a_decision_notification_from_aws_sqs_Then_resolved_messaged_should_be_received()
    {
        var semaphore = new SemaphoreSlim(1, 1);
        var id = Guid.NewGuid().ToString();

        _awsConsumers.ConsumerMock.SetOnHandle = () => semaphore.Release();

        await _awsSender.SendAsync(new Decision { ServiceHeader = new ServiceHeader { CorrelationId = id } });

        var received = await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
        received.Should().BeTrue();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _awsSender.DisposeAsync();
        await _awsConsumers.DisposeAsync();
    }
}
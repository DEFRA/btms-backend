using Btms.Types.Alvs;
using FluentAssertions;
using NSubstitute;
using SlimMessageBus;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

[Collection("AwsSqsSns")]
public class HmrcClearanceRequestConsumerTests : IAsyncLifetime
{
    private readonly TestAwsConsumers _awsConsumers = new();
    private readonly TestAwsSender _awsSender;

    public HmrcClearanceRequestConsumerTests(ITestOutputHelper testOutputHelper)
    {
        _awsSender = new TestAwsSender(_awsConsumers.Configuration, _awsConsumers.AwsLocalOptions, testOutputHelper);
    }

    [Fact]
    public async Task When_receiving_a_clearance_request_from_aws_sqs_Then_resolved_messaged_should_be_received()
    {
        var semaphore = new SemaphoreSlim(1, 1);
        var id = Guid.NewGuid().ToString();

        _awsConsumers.ClearanceRequestConsumer.Mock
            .When(mock => mock.OnHandle(Arg.Is<AlvsClearanceRequest>(a => a.ServiceHeader != null && a.ServiceHeader.CorrelationId == id), Arg.Any<IConsumerContext>(), Arg.Any<CancellationToken>()))
            .Do(_=> semaphore.Release());

        await _awsSender.SendAsync(new AlvsClearanceRequest { ServiceHeader = new ServiceHeader { CorrelationId = id } });

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
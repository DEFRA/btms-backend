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

    // [Fact(Skip = "Can't get working on the server but need to test deployment")]
    [Fact]
    public async Task When_receiving_a_clearance_request_from_aws_sqs_Then_resolved_messaged_should_be_received()
    {
        await _awsSender.SendAsync(new AlvsClearanceRequest { ServiceHeader = new ServiceHeader { CorrelationId = "abc" } });

        _awsConsumers.ClearanceRequestConsumer.WaitUntilHandled().Should().BeTrue(because: "The message was not handled by the consumer");

        await _awsConsumers.ClearanceRequestConsumer.Mock.Received().OnHandle(Arg.Is<AlvsClearanceRequest>(a => a.ServiceHeader != null && a.ServiceHeader.CorrelationId == "abc"), Arg.Any<IConsumerContext>(), Arg.Any<CancellationToken>());
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _awsSender.DisposeAsync();
        await _awsConsumers.DisposeAsync();
    }
}
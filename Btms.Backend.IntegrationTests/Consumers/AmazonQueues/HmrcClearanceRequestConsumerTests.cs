using Btms.Types.Alvs;
using FluentAssertions;
using NSubstitute;
using SlimMessageBus;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public class HmrcClearanceRequestConsumerTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task When_receiving_a_clearance_request_from_aws_sqs_Then_resolved_messaged_should_be_received()
    {
        var awsConsumers = new TestAwsConsumers();
        var awsSender = new TestAwsSender(awsConsumers.Configuration);

        await awsSender.SendAsync(new AlvsClearanceRequest { ServiceHeader = new ServiceHeader { CorrelationId = "abc" } }, testOutputHelper);

        awsConsumers.ClearanceRequestConsumer.WaitUntilHandled().Should().BeTrue(because: "The message was not handled by the consumer");

        await awsConsumers.ClearanceRequestConsumer.Mock.Received().OnHandle(Arg.Is<AlvsClearanceRequest>(a => a.ServiceHeader != null && a.ServiceHeader.CorrelationId == "abc"), Arg.Any<IConsumerContext>(), Arg.Any<CancellationToken>());
    }
}
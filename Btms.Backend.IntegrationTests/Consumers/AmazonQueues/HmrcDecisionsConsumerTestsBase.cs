using Btms.Types.Alvs;
using FluentAssertions;
using NSubstitute;
using SlimMessageBus;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

[Collection("AwsSqsSns")]
[Trait("Category", "Integration")]
public class HmrcDecisionsConsumerTestsBase(ITestOutputHelper testOutputHelper) : AwsConsumerTestsBase(testOutputHelper)
{
    [Fact]
    public async Task When_receiving_a_decision_notification_from_aws_sqs_Then_resolved_messaged_should_be_received()
    {
        var semaphore = new SemaphoreSlim(1, 1);
        var id = Guid.NewGuid().ToString();

        AwsConsumers.DecisionsConsumerMock
            .When(mock => mock.OnHandle(Arg.Is<Decision>(a => a.ServiceHeader != null && a.ServiceHeader.CorrelationId == id), Arg.Any<IConsumerContext>(), Arg.Any<CancellationToken>()))
            .Do(_ => semaphore.Release());

        await AwsSender.SendAsync(new Decision { ServiceHeader = new ServiceHeader { CorrelationId = id } }, AwsConsumers.AwsSqsOptions.DecisionsQueueName);

        var received = await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
        received.Should().BeTrue();
    }
}
using Btms.Types.Alvs;
using FluentAssertions;
using NSubstitute;
using SlimMessageBus;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

[Collection("AwsSqsSns")]
[Trait("Category", "Integration")]
public class HmrcClearanceRequestConsumerTestsBase(ITestOutputHelper testOutputHelper) : AwsConsumerTestsBase(testOutputHelper)
{
    [Fact]
    public async Task WhenReceivingClearanceRequestFromAwsSqsThenResolvedMessagedShouldBeReceived()
    {
        var semaphore = new SemaphoreSlim(1, 1);
        var id = Guid.NewGuid().ToString();

        AwsConsumers.ClearanceRequestConsumerMock
            .When(mock => mock.OnHandle(Arg.Is<AlvsClearanceRequest>(a => a.ServiceHeader != null && a.ServiceHeader.CorrelationId == id), Arg.Any<IConsumerContext>(), Arg.Any<CancellationToken>()))
            .Do(_ => semaphore.Release());

        await AwsSender.SendAsync(new AlvsClearanceRequest { ServiceHeader = new ServiceHeader { CorrelationId = id } }, AwsConsumers.AwsSqsOptions.ClearanceRequestQueueName);

        var received = await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
        received.Should().BeTrue();
    }
}
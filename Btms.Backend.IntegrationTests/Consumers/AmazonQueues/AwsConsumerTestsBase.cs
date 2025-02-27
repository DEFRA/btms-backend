using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public abstract class AwsConsumerTestsBase : IAsyncLifetime
{
    protected readonly TestAwsConsumers AwsConsumers = new();
    protected readonly TestAwsSender AwsSender;

    protected AwsConsumerTestsBase(ITestOutputHelper testOutputHelper)
    {
        AwsSender = new TestAwsSender(AwsConsumers.Configuration, AwsConsumers.AwsSqsOptions, testOutputHelper);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await AwsSender.DisposeAsync();
        await AwsConsumers.DisposeAsync();
    }
}
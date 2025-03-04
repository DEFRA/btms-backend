using Btms.Common.Extensions;
using Btms.Consumers.AmazonQueues;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlimMessageBus;
using SlimMessageBus.Host;
using SlimMessageBus.Host.AmazonSQS;
using SlimMessageBus.Host.Serialization.SystemTextJson;
namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;
public class MockClearanceRequestConsumer : IConsumer<MessageBody>, IConsumerWithContext
{
    public Task OnHandle(MessageBody message, CancellationToken cancellationToken)
    {
        if (SetOnHandle != null)
        {
            SetOnHandle();
        }

        return Task.CompletedTask;
    }

    public IConsumerContext Context { get; set; } = null!;

    public Action SetOnHandle = null!;
}

public class TestAwsConsumers : IAsyncDisposable
{
    private readonly WebApplication _app = null!;
    private readonly CancellationTokenSource _tokenSource = new();

    public readonly MockClearanceRequestConsumer ClearanceRequestConsumerMock = new();
    public readonly IConfiguration Configuration;
    public readonly AwsSqsOptions AwsSqsOptions;

    public TestAwsConsumers()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddInMemoryCollection(AwsConfig.DefaultLocalConfig);

        Configuration = builder.Configuration;
        AwsSqsOptions = builder.Services.BtmsAddOptions<AwsSqsOptions>(Configuration, AwsSqsOptions.SectionName).Get();
        builder.Services.AddScoped<MockClearanceRequestConsumer>(_ => ClearanceRequestConsumerMock);

        try
        {
            builder.Services.AddSlimMessageBus(mbb =>
            {
                mbb.AddChildBus("AmazonTest", cbb =>
                {
                    mbb.WithProviderAmazonSQS(cfg =>
                    {
                        cfg.TopologyProvisioning.Enabled = false;
                        cfg.SqsClientConfig.ServiceURL = AwsSqsOptions.ServiceUrl;
                        cfg.UseCredentials(AwsSqsOptions.AccessKeyId, AwsSqsOptions.SecretAccessKey);
                    });

                    mbb.AddJsonSerializer();
                    mbb.Consume<MessageBody>(x => x
                        .WithConsumer<MockClearanceRequestConsumer>()
                        .Queue(AwsSqsOptions.ClearanceRequestQueueName));
                });
            });

            _app = builder.Build();

            Task.Run(() => _app.Start(), _tokenSource.Token);
            Task.Delay(TimeSpan.FromSeconds(5), _app.Lifetime.ApplicationStarted).Wait();

            throw new TimeoutException("Unable to start the AWS SNS/SQS test service in the allotted time");
        }
        catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
        {
            // Expected
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to build and start the AWS SNS/SQS test service", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _tokenSource.CancelAsync();
        await _app.StopAsync();
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), _app.Lifetime.ApplicationStopped);
        }
        catch (TaskCanceledException)
        {
            // Expected
        }
    }
}
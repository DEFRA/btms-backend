using Btms.Common.Extensions;
using Btms.Consumers;
using Btms.Consumers.AmazonQueues;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Serilog.Core;
using SlimMessageBus.Host;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public class TestAwsConsumers : IAsyncDisposable
{
    private readonly WebApplication _app = null!;
    private readonly CancellationTokenSource _tokenSource = new();

    public readonly IClearanceRequestConsumer ClearanceRequestConsumerMock = Substitute.For<IClearanceRequestConsumer>();
    public readonly IConfiguration Configuration;
    public readonly AwsSqsOptions AwsSqsOptions;

    public TestAwsConsumers()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddInMemoryCollection(AwsConfig.DefaultLocalConfig);

        Configuration = builder.Configuration;
        AwsSqsOptions = builder.Services.BtmsAddOptions<AwsSqsOptions>(Configuration, AwsSqsOptions.SectionName).Get();

        try
        {
            builder.Services.AddScoped<IClearanceRequestConsumer>(_ => ClearanceRequestConsumerMock);
            builder.Services.AddSlimMessageBus(mbb =>
            {
                mbb.AddChildBus("AmazonTest", cbb =>
                {
                    cbb.AddAmazonConsumers(builder.Services, AwsSqsOptions, Logger.None);
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
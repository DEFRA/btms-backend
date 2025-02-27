using System.Diagnostics;
using Btms.Common.Extensions;
using Btms.Consumers;
using Btms.Consumers.AmazonQueues;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using SlimMessageBus.Host;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public class TestAwsConsumers : IAsyncDisposable
{
    private readonly WebApplication _app = null!;
    private readonly CancellationTokenSource _tokenSource = new();

    public readonly ClearanceRequestConsumerHost ClearanceRequestConsumer = new();
    public readonly IConfiguration Configuration;
    public readonly AwsSqsOptions AwsLocalOptions;

    public TestAwsConsumers()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Configuration
            .AddEnvironmentVariables()
            .AddInMemoryCollection(AwsConfig.DefaultLocalConfig);

        Configuration = builder.Configuration;
        AwsLocalOptions = builder.Services.BtmsAddOptions<AwsSqsOptions>(Configuration, AwsSqsOptions.SectionName).Get();

        try
        {
            builder.Services.AddScoped<IClearanceRequestConsumer>(_ => ClearanceRequestConsumer.Mock);
            builder.Services.AddSlimMessageBus(mbb =>
            {
                mbb.AddChildBus("AmazonTest", cbb =>
                {
                    cbb.AddAmazonConsumers(builder.Services, AwsLocalOptions);
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

public class ClearanceRequestConsumerHost : ConsumerHost<IClearanceRequestConsumer>
{
}

public abstract class ConsumerHost<T> where T : class
{
    public readonly T Mock = Substitute.For<T>();

    private SpinWait _spinner;

    public bool WaitUntil(Func<bool> actionToAwait)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            if (actionToAwait())
                return true;

            _spinner.SpinOnce();
        }

        return false;
    }
}
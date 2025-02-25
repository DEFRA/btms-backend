using System.Diagnostics;
using Btms.Backend.Utils.Logging;
using Btms.Consumers;
using Btms.Consumers.AmazonQueues;
using Btms.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Exceptions;
using Serilog;
using SlimMessageBus.Host;

namespace Btms.Backend.IntegrationTests.Consumers.AmazonQueues;

public class TestAwsConsumers : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly CancellationTokenSource _tokenSource = new();
    public readonly ClearanceRequestConsumerHost ClearanceRequestConsumer = new();
    public readonly IConfiguration Configuration;

    public TestAwsConsumers()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddIniFile("Properties/local.env", true)
            .AddEnvironmentVariables();

        builder.Logging.ClearProviders();
        var logBuilder = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.With<LogLevelMapper>();
        var logger = logBuilder.CreateLogger();
        builder.Logging.AddSerilog(logger);

        builder.Services.AddScoped<IClearanceRequestConsumer>(_ => ClearanceRequestConsumer.Mock);
        builder.Services.AddSlimMessageBus(mbb =>
        {
            mbb.AddChildBus("AmazonTest", cbb =>
            {
                cbb.AddAmazonConsumers(builder.Services, builder.Configuration, logger);
            });
        });

        _app = builder.Build();

        Configuration = _app.Configuration;

        Task.Run(() => _app.Start(), _tokenSource.Token);
    }

    public async ValueTask DisposeAsync()
    {
        await _tokenSource.CancelAsync();
        await _app.StopAsync();
    }
}

public class ClearanceRequestConsumerHost : ConsumerHost<IClearanceRequestConsumer>
{
    public bool WaitUntilHandled() => WaitUntilHandledAsync(() => Mock.ReceivedWithAnyArgs().OnHandle(default!, default!, default)).Result;
}

public abstract class ConsumerHost<T> where T : class
{
    public readonly T Mock = Substitute.For<T>();

    private SpinWait _spinner;

    protected async Task<bool> WaitUntilHandledAsync(Func<Task> actionToAwait)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            try
            {
                await actionToAwait();
                return true;
            }
            catch (ReceivedCallsException)
            {
                _spinner.SpinOnce();
            }
        }

        return false;
    }
}
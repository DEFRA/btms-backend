using Btms.Metrics;
using System.Diagnostics;

namespace Btms.Backend.BackgroundTaskQueue;

internal class QueueHostedService : BackgroundService
{
    public const string ActivityName = "Btms.Job.Run";
    private readonly ILogger<QueueHostedService> _logger;

    public QueueHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueueHostedService> logger)
    {
        TaskQueue = taskQueue;
        _logger = logger;
    }

    public IBackgroundTaskQueue TaskQueue { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is running...");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await TaskQueue.DequeueAsync(stoppingToken);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    _logger.LogInformation("Starting execution of {WorkItem}...", nameof(workItem));
                    using (BtmsDiagnostics.ActivitySource.StartActivity(ActivityName, ActivityKind.Client))
                    {
                        workItem(stoppingToken).GetAwaiter().GetResult();
                    }

                    _logger.LogInformation("Execution of {WorkItem} completed!!!", nameof(workItem));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occured executing {WorkItem}", nameof(workItem));
                }
            });


        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Queue Hosted service has been stopped!!!");

        await base.StopAsync(cancellationToken);
    }
}
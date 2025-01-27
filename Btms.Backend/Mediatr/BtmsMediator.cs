using Btms.SyncJob;
using MediatR;
using System.Diagnostics;
using Btms.Backend.BackgroundTaskQueue;
using Btms.Business.Mediatr;

namespace Btms.Backend.Mediatr;

internal class BtmsMediator(
    IBackgroundTaskQueue backgroundTaskQueue,
    IServiceScopeFactory serviceScopeFactory,
    IMediator mediator,
    ISyncJobStore syncJobStore)
    : IBtmsMediator
{
    internal static readonly string ActivitySourceName = "Btms";
    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName, "1.0");
    public const string ActivityName = "BtmsMediator.Queue.Background";

    public async Task SendSyncJob<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest, ISyncJob
    {
        var job = syncJobStore.CreateJob(request.JobId, request.Timespan, request.Resource);

        await backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (_) =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            using var activity = ActivitySource.StartActivity(ActivityName, ActivityKind.Client);
            var m = scope.ServiceProvider.GetRequiredService<IMediator>();
            await m.Send(request, job.CancellationToken);
        });
    }

    public async Task SendJob<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest, ISyncJob
    {
        var job = syncJobStore.CreateJob(request.JobId, request.Timespan, request.Resource);

        await backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (_) =>
        {
            job.Start();
            using var scope = serviceScopeFactory.CreateScope();
            using var activity = ActivitySource.StartActivity(ActivityName, ActivityKind.Client);
            var m = scope.ServiceProvider.GetRequiredService<IMediator>();
            await m.Send(request, job.CancellationToken);
            job.Complete();
        });
    }

    Task<TResponse> IBtmsMediator.Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        return mediator.Send(request, cancellationToken);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        return mediator.Send(request, cancellationToken);
    }
}
using Cdms.SyncJob;
using CdmsBackend.BackgroundTaskQueue;
using MediatR;
using System.Diagnostics;
using System.Reflection;

namespace CdmsBackend.Mediatr
{
    internal class CdmsMediator(
        IBackgroundTaskQueue backgroundTaskQueue,
        IServiceScopeFactory serviceScopeFactory,
        IMediator mediator,
        ISyncJobStore syncJobStore)
        : ICdmsMediator
    {
        internal static readonly AssemblyName AssemblyName = typeof(CdmsMediator).Assembly.GetName();
        internal static readonly string ActivitySourceName = "Cdms";
        internal static readonly ActivitySource ActivitySource = new(ActivitySourceName, "1.0");
        public const string ActivityName = "CdmsMediator.Queue.Background";

        public async Task SendSyncJob<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest, ISyncJob
        {
            syncJobStore.CreateJob(request.JobId, request.Description);
            
            await backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (stoppingToken) =>
            {
                using var scope = serviceScopeFactory.CreateScope();
                using var activity = ActivitySource.StartActivity(ActivityName, ActivityKind.Client);
                var m = scope.ServiceProvider.GetRequiredService<IMediator>();
                await m.Send(request, stoppingToken);
            });
        }

        Task<TResponse> ICdmsMediator.Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return mediator.Send(request, cancellationToken);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            return mediator.Send(request, cancellationToken);
        }
    }
}

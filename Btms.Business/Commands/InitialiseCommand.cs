using Btms.BlobService;
using Btms.Business.Mediatr;
using Btms.Metrics;
using Btms.SensitiveData;
using Btms.SyncJob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Btms.Business.Commands;

public class InitialiseCommand : SyncCommand
{
    internal class Handler(
        SyncMetrics syncMetrics,
        IPublishBus bus,
        ILogger<InitialiseCommand> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService,
        IOptions<BusinessOptions> businessOptions,
        IBtmsMediator mediator,
        ISyncJobStore syncJobStore)
        : Handler<InitialiseCommand>(syncMetrics, bus, logger, sensitiveDataSerializer, blobService, businessOptions, syncJobStore)
    {
        public override async Task Handle(InitialiseCommand request, CancellationToken cancellationToken)
        {
            var job = SyncJobStore.GetJob(request.JobId)!;
            
            SyncClearanceRequestsCommand clearanceRequests = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(clearanceRequests, cancellationToken);

            SyncNotificationsCommand notifications = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(notifications, cancellationToken);
            
            Logger.LogInformation("Started Notifications {0} and ClearanceRequests {1} sync jobs. Waiting on ClearanceRequests job to complete.", notifications.JobId, clearanceRequests.JobId);

            job.Start();

            await SyncJobStore.WaitOnJobCompleting(clearanceRequests.JobId);
            
            SyncDecisionsCommand decisions = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(decisions, cancellationToken);

            SyncFinalisationsCommand finalisations = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(finalisations, cancellationToken);

            Logger.LogInformation("ClearanceRequests sync job complete. Started Decisions {0} and Finalisations {1} sync jobs", decisions.JobId, finalisations.JobId);

            await Task.WhenAll(
                SyncJobStore.WaitOnJobCompleting(decisions.JobId),
                SyncJobStore.WaitOnJobCompleting(finalisations.JobId),
                SyncJobStore.WaitOnJobCompleting(notifications.JobId));
            
            job.Complete();
        }
    }
    
    public override string Resource => "Initialise";
}
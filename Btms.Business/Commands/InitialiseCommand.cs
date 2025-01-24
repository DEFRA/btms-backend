using System.Text.Json.Serialization;
using Btms.BlobService;
using Btms.Business.Mediatr;
using Btms.Common.Extensions;
using Btms.Metrics;
using Btms.SensitiveData;
using Btms.SyncJob;
using Btms.Types.Alvs;
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
            
            var isComplete = false;
            
            while(!isComplete)
            {
                var clearanceRequestJob = SyncJobStore.GetJob(clearanceRequests.JobId)!;

                Logger.LogInformation("ClearanceRequests sync jobs status {0}", clearanceRequestJob.Status);
                
                if (!clearanceRequestJob.Status.IsAny(SyncJobStatus.Running, SyncJobStatus.Pending))
                {
                    isComplete = true;
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
            
            SyncDecisionsCommand decisions = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(decisions, cancellationToken);

            SyncFinalisationsCommand finalisations = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(finalisations, cancellationToken);

            Logger.LogInformation("ClearanceRequests sync job complete. Started Decisions {0} and Finalisations {1} sync jobs", decisions.JobId, finalisations.JobId);
            
            isComplete = false;
            
            while(!isComplete)
            {
                var decisionsJob = SyncJobStore.GetJob(decisions.JobId)!;
                var finalisationsJob = SyncJobStore.GetJob(finalisations.JobId)!;
                var notificationsJob = SyncJobStore.GetJob(notifications.JobId)!;

                Logger.LogInformation("Decisions {0}, Finalisations {1}, Notifications {2}", decisionsJob.Status, finalisationsJob.Status, notificationsJob.Status);
                
                if (!decisionsJob.Status.IsAny(SyncJobStatus.Running, SyncJobStatus.Pending) &&
                    !finalisationsJob.Status.IsAny(SyncJobStatus.Running, SyncJobStatus.Pending) &&
                    !notificationsJob.Status.IsAny(SyncJobStatus.Running, SyncJobStatus.Pending))
                {
                    isComplete = true;
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
            
            job.Complete();
        }
    }
    
    public override string Resource => "Initialise";
}
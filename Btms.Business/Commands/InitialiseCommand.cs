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

            Logger.LogInformation("ClearanceRequests Sync job started {0}", clearanceRequests.JobId);
            
            SyncNotificationsCommand notifications = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(notifications, cancellationToken);
            
            Logger.LogInformation("Notifications Sync job started {0}", notifications.JobId);

            job.Start();
            
            var isComplete = false;
            
            while(!isComplete)
            {
                var clearanceRequestJob = SyncJobStore.GetJob(clearanceRequests.JobId)!;

                Logger.LogInformation("Sync jobs status clearance requests {0}", clearanceRequestJob.Status);
                
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

            Logger.LogInformation("ClearanceRequests Sync job started {0}", clearanceRequests.JobId);
            
            SyncFinalisationsCommand finalisations = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(finalisations, cancellationToken);

            isComplete = false;
            
            while(!isComplete)
            {
                var decisionsJob = SyncJobStore.GetJob(decisions.JobId)!;
                var finalisationsJob = SyncJobStore.GetJob(finalisations.JobId)!;

                Logger.LogInformation("Sync jobs status decisions {0}, finalisations {1}", decisionsJob.Status, finalisationsJob.Status);
                
                if (!decisionsJob.Status.IsAny(SyncJobStatus.Running, SyncJobStatus.Pending) &&
                    !finalisationsJob.Status.IsAny(SyncJobStatus.Running, SyncJobStatus.Pending))
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
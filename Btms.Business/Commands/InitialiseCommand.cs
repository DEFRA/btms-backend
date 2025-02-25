using Btms.BlobService;
using Btms.Business.Mediatr;
using Btms.Metrics;
using Btms.SensitiveData;
using Btms.SyncJob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using SlimMessageBus;
using Btms.Common.FeatureFlags;

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
        ISyncJobStore syncJobStore,
        IFeatureManager featureManager)
        : Handler<InitialiseCommand>(syncMetrics, bus, logger, sensitiveDataSerializer, blobService, businessOptions,
            syncJobStore)
    {
        public override async Task Handle(InitialiseCommand request, CancellationToken cancellationToken)
        {
            var job = SyncJobStore.GetJob(request.JobId)!;
            job.Start();

            SyncNotificationsCommand notifications = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(notifications, cancellationToken);

            Logger.LogInformation("Started Notifications {0} sync job. Waiting on Notifications job to complete.",
                notifications.JobId);

            if (await featureManager.IsEnabledAsync(Features.SyncPerformanceEnhancements))
            {
                await SyncJobStore.WaitOnJobCompleting(notifications.JobId);
            }

            SyncClearanceRequestsCommand clearanceRequests = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(clearanceRequests, cancellationToken);

            Logger.LogInformation(
                "Started ClearanceRequests {0} sync jobs. Waiting on ClearanceRequests job to complete.",
                clearanceRequests.JobId);

            await SyncJobStore.WaitOnJobCompleting(clearanceRequests.JobId);

            var decisions = RunDecisionsAndFinalisations(request, cancellationToken);

            await Task.WhenAll(
                decisions,
                SyncJobStore.WaitOnJobCompleting(notifications.JobId));

            job.Complete();
        }

        private async Task RunDecisionsAndFinalisations(InitialiseCommand request, CancellationToken cancellationToken)
        {
            SyncDecisionsCommand decisions = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(decisions, cancellationToken);

            SyncFinalisationsCommand finalisations = new() { SyncPeriod = request.SyncPeriod };
            await mediator.SendSyncJob(finalisations, cancellationToken);

            Logger.LogInformation("Started Decisions {0} and Finalisations {1} sync jobs", decisions.JobId,
                finalisations.JobId);

            await Task.WhenAll(
                SyncJobStore.WaitOnJobCompleting(decisions.JobId),
                SyncJobStore.WaitOnJobCompleting(finalisations.JobId));
        }
    }

    public override string Resource => "Initialise";
}
using System.Globalization;
using Btms.Backend.Data;
using Btms.BlobService;
using Btms.Business.Mediatr;
using Btms.Common.Extensions;
using Btms.Metrics;
using Btms.SensitiveData;
using Btms.SyncJob;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Btms.Business.Commands;

public enum InitialisationStrategy
{
    AllSinceNovember,
    AllSinceNovemberImportNotificationsFirst,
    ImportNotificationsFirst
}
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
        IMongoDbContext context)
        : Handler<InitialiseCommand>(syncMetrics, bus, logger, sensitiveDataSerializer, blobService, businessOptions, syncJobStore)
    {
        public override async Task Handle(InitialiseCommand request, CancellationToken cancellationToken)
        {
            var job = SyncJobStore.GetJob(request.JobId)!;
            job.Start();

            InitialisationStrategy[] sinceNovemberStrategies =
            [
                InitialisationStrategy.AllSinceNovemberImportNotificationsFirst,
                InitialisationStrategy.AllSinceNovember
            ];

            List<string?> datasets = !request.RootFolder.HasValue() ? [null] : [request.RootFolder!];

            if (request.Strategy.HasValue() && sinceNovemberStrategies.Contains(request.Strategy!.Value))
            {
                datasets = DateTimeExtensions.RedactedDatasetsSinceNov24()
                    .Cast<string?>()
                    .ToList();
            }

            InitialisationStrategy[] importNotificationsFirstStrategies =
            [
                InitialisationStrategy.AllSinceNovemberImportNotificationsFirst,
                InitialisationStrategy.ImportNotificationsFirst
            ];

            if (request.DropCollections)
            {
                SyncJobStore.ClearSyncJobs();
                await context.ResetCollections(cancellationToken);
            }

            if (request.Strategy.HasValue() && importNotificationsFirstStrategies.Contains(request.Strategy!.Value))
            {
                Logger.LogInformation("ImportNotificationsFirst Strategy");

                await datasets.ForEachAsync(async ds =>
                {
                    SyncNotificationsCommand notifications = new() { SyncPeriod = request.SyncPeriod, RootFolder = ds };
                    await mediator.SendSyncJob(notifications, cancellationToken);

                    Logger.LogInformation("Started Notifications {JobId} job. Waiting on completion.", notifications.JobId);
                    await SyncJobStore.WaitOnJobCompleting(notifications.JobId);

                });

                await datasets.ForEachAsync(async ds =>
                {
                    SyncClearanceRequestsCommand clearanceRequests = new() { SyncPeriod = request.SyncPeriod, RootFolder = ds };
                    await mediator.SendSyncJob(clearanceRequests, cancellationToken);

                    Logger.LogInformation("Started Clearance Requests sync job {JobId}. Waiting on completion.", clearanceRequests.JobId);

                    await SyncJobStore.WaitOnJobCompleting(clearanceRequests.JobId);

                });

                await datasets.ForEachAsync(async ds =>
                {
                    SyncDecisionsCommand decisions = new() { SyncPeriod = request.SyncPeriod, RootFolder = ds };
                    await mediator.SendSyncJob(decisions, cancellationToken);

                    Logger.LogInformation("Started Decisions sync job {JobId}. Waiting on completion.", decisions.JobId);

                    await SyncJobStore.WaitOnJobCompleting(decisions.JobId);
                });

                await datasets.ForEachAsync(async ds =>
                {
                    SyncFinalisationsCommand finalisations = new() { SyncPeriod = request.SyncPeriod, RootFolder = ds };
                    await mediator.SendSyncJob(finalisations, cancellationToken);

                    Logger.LogInformation("Started Finalisations sync job {JobId} for {Dataset}. Waiting on completion.", finalisations.JobId, ds);

                    await SyncJobStore.WaitOnJobCompleting(finalisations.JobId);
                });
            }
            else
            {
                Logger.LogInformation("Run a standard sync of a single dataset");
                await datasets.ForEachAsync(async ds => await Simple(request.SyncPeriod, ds, cancellationToken));
            }

            job.Complete();
        }

        private async Task Simple(SyncPeriod syncPeriod, string? rootFolder, CancellationToken cancellationToken)
        {
            SyncClearanceRequestsCommand clearanceRequests = new() { SyncPeriod = syncPeriod, RootFolder = rootFolder };
            await mediator.SendSyncJob(clearanceRequests, cancellationToken);

            SyncNotificationsCommand notifications = new() { SyncPeriod = syncPeriod, RootFolder = rootFolder };
            await mediator.SendSyncJob(notifications, cancellationToken);

            Logger.LogInformation("Started Notifications {NotificationsJobId} and ClearanceRequests {ClearanceRequestsJobId} sync jobs. Waiting on ClearanceRequests job to complete.",
                notifications.JobId, clearanceRequests.JobId);

            await SyncJobStore.WaitOnJobCompleting(clearanceRequests.JobId);

            SyncDecisionsCommand decisions = new() { SyncPeriod = syncPeriod, RootFolder = rootFolder };
            await mediator.SendSyncJob(decisions, cancellationToken);

            SyncFinalisationsCommand finalisations = new() { SyncPeriod = syncPeriod, RootFolder = rootFolder };
            await mediator.SendSyncJob(finalisations, cancellationToken);

            Logger.LogInformation("ClearanceRequests sync job complete. Started Decisions {DecisionsJobId} and Finalisations {FinalisationsJobId} sync jobs", decisions.JobId, finalisations.JobId);

            await Task.WhenAll(
                SyncJobStore.WaitOnJobCompleting(decisions.JobId),
                SyncJobStore.WaitOnJobCompleting(finalisations.JobId),
                SyncJobStore.WaitOnJobCompleting(notifications.JobId));
        }
    }


    public override string Resource => "Initialise";
    public InitialisationStrategy? Strategy { get; init; }
    public bool DropCollections { get; init; } = true;
}
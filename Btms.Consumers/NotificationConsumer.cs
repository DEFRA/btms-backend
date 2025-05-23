using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Btms.Backend.Data;
using Btms.Backend.Data.Extensions;
using Btms.Types.Ipaffs;
using SlimMessageBus;
using Btms.Consumers.Extensions;
using Microsoft.Extensions.Logging;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Business.Services.Matching;
using Btms.Business.Services.Validating;
using Btms.Model.Gvms;
using DecisionContext = Btms.Business.Services.Decisions.DecisionContext;
using Btms.Business.Builders;
using Btms.Types.Alvs.Mapping;
using AsyncKeyedLock;
using System.Threading;
using Microsoft.FeatureManagement;
using Btms.Common.FeatureFlags;

namespace Btms.Consumers;

[SuppressMessage("SonarLint", "S107",
    Justification =
        "The consumer is orchestrating different service calls therefore inherently has too many responsibilities")]
internal class NotificationConsumer(
    IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification> preProcessor,
    ILinkingService linkingService,
    IMatchingService matchingService,
    IDecisionService decisionService,
    IValidationService validationService,
    ILogger<NotificationConsumer> logger,
    IMongoDbContext dbContext,
    ILinker<Gmr, Model.Ipaffs.ImportNotification> gmrLinker,
    IFeatureManager featureManager)
    : IConsumer<ImportNotification>, IConsumerWithContext
{
    private static readonly AsyncKeyedLocker<string> _asyncKeyedLocker = new();
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    public async Task OnHandle(ImportNotification message, CancellationToken cancellationToken)
    {
        if (await featureManager.IsEnabledAsync(Features.SyncPerformanceEnhancements))
        {
            IDisposable? asyncLock = null;
            try
            {
                if (Context.UseLock())
                {
                    asyncLock = await _asyncKeyedLocker.LockOrNullAsync(message.ReferenceNumber!, _timeout,
                        cancellationToken);
                }

                await Process(message, cancellationToken);
            }
            finally
            {
                asyncLock?.Dispose();
            }

            return;
        }

        await Process(message, cancellationToken);
    }


    private async Task Process(ImportNotification message, CancellationToken cancellationToken)
    {
        var messageId = Context.GetMessageId();
        using (logger.BeginScope(Context.GetJobId()!, messageId, GetType().Name, message.ReferenceNumber!))
        {
            var preProcessingResult = await preProcessor
                .Process(new PreProcessingContext<ImportNotification>(message, messageId));

            if (preProcessingResult.Outcome == PreProcessingOutcome.NotProcessed)
            {
                LogStatus("Not Processed due to being in DRAFT or AMEND state", message);
                return;
            }

            if (preProcessingResult.Outcome == PreProcessingOutcome.Skipped)
            {
                LogStatus("Skipped", message);
                Context.Skipped();
            }
            else
            {
                LogStatus("PreProcessed", message);
                Context.PreProcessed();
            }

            if (preProcessingResult.IsCreatedOrChanged())
            {
                LogStatus("IsCreatedOrChanged=true", message);

                var linkContext = new ImportNotificationLinkContext(preProcessingResult.Record,
                    preProcessingResult.ChangeSet);
                var linkResult = await linkingService.Link(linkContext, Context.CancellationToken);

                if (linkResult.Outcome != LinkOutcome.NotLinked)
                {
                    LogStatus("Linked", message);
                    Context.Linked();
                }

                await gmrLinker.Link(preProcessingResult.Record, cancellationToken);

                if (!await validationService.PostLinking(linkContext, linkResult,
                        triggeringNotification: preProcessingResult.Record,
                        cancellationToken: Context.CancellationToken))
                {
                    logger.LogWarning(
                        "Skipping Matching/Decisions due to PostLinking failure for {Id} with MessageId {MessageId}",
                        message.ReferenceNumber, messageId);
                    await dbContext.SaveChangesAsync(cancellation: Context.CancellationToken);
                    return;
                }

                var notifications = await LoadAllNotificationReferenced(cancellationToken, linkResult);

                var matchResult = await matchingService.Process(
                    new MatchingContext(notifications, linkResult.Movements), Context.CancellationToken);

                var decisionContext = new DecisionContext(notifications, linkResult.Movements, matchResult, messageId);
                var decisionResult = await decisionService.Process(decisionContext, Context.CancellationToken);

                await validationService.PostDecision(decisionContext, decisionResult, Context.CancellationToken);
            }
            else if (preProcessingResult.IsCancelledOrDeleted())
            {
                // This would happen from a decision, but for cancellation or deleted updates, no decision
                // is made.
                LogStatus("Notification is CancelledOrDeleted, update Movements ImportNotificationState", message);

                var linkContext = new ImportNotificationLinkContext(preProcessingResult.Record,
                    preProcessingResult.ChangeSet);
                var linkResult = await linkingService.Link(linkContext, Context.CancellationToken);

                dbContext.Movements.UpdateAlvsDecisionStatusImportNotificationState(linkResult.Movements,
                    preProcessingResult.Record);

                if (preProcessingResult.IsDeleted())
                {
                    linkContext = new ImportNotificationLinkContext(preProcessingResult.Record,
                        preProcessingResult.ChangeSet, linkResult.Movements);
                    await linkingService.UnLink(linkContext, Context.CancellationToken);
                }
            }
            else
            {
                logger.LogWarning(
                    "Skipping Linking/Matching/Decisions for {Id} with MessageId {MessageId} with Pre-Processing Outcome {PreProcessingOutcome} Because Last AuditState was {AuditState}",
                    message.ReferenceNumber, messageId, preProcessingResult.Outcome.ToString(),
                    preProcessingResult.Record.GetLatestAuditEntry().Status);
                LogStatus("IsCreatedOrChanged=false", message);
            }

            preProcessingResult.Record.CalculateStatus();

            await dbContext.SaveChangesAsync(cancellation: Context.CancellationToken);
        }
    }

    private async Task<List<Model.Ipaffs.ImportNotification>> LoadAllNotificationReferenced(
        CancellationToken cancellationToken, LinkResult linkResult)
    {
        var movementMatchReferences = linkResult.Movements.SelectMany(x => x._MatchReferences).ToList();
        var notificationIdentifiers = linkResult.Notifications.Select(x => x._MatchReference);
        var missingIdentifiers = movementMatchReferences.Except(notificationIdentifiers).ToList();
        var notifications = await dbContext.Notifications
            .Where(x => missingIdentifiers.Contains(x._MatchReference))
            .ToListAsync(cancellationToken);
        notifications.AddRange(linkResult.Notifications);
        return notifications;
    }

    private void LogStatus(string state, ImportNotification message)
    {
        logger.LogInformation("{state} : {id}, {version}, {lastUpdated}", state, message.ReferenceNumber,
            message.Version, message.LastUpdated);
    }

    public IConsumerContext Context { get; set; } = null!;
}
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
using Btms.Model.Cds;
using DecisionContext = Btms.Business.Services.Decisions.DecisionContext;
using Btms.Business.Builders;
using Btms.Types.Alvs.Mapping;

namespace Btms.Consumers;

internal class NotificationConsumer(IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification> preProcessor, ILinkingService linkingService,
    IMatchingService matchingService,
    IDecisionService decisionService,
    IValidationService validationService,
    ILogger<NotificationConsumer> logger,
    IMongoDbContext dbContext)
: IConsumer<ImportNotification>, IConsumerWithContext
{
    public async Task OnHandle(ImportNotification message, CancellationToken cancellationToken)
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
                // 
                if (!await validationService.PostLinking(linkContext, linkResult,
                        triggeringNotification: preProcessingResult.Record,
                        cancellationToken: Context.CancellationToken))
                {
                    logger.LogWarning("Skipping Matching/Decisions due to PostLinking failure for {Id} with MessageId {MessageId}", message.ReferenceNumber, messageId);
                    await dbContext.SaveChangesAsync(Context.CancellationToken);
                    return;
                }

                var notifications = await LoadAllNotificationReferenced(cancellationToken, linkResult);

                var matchResult = await matchingService.Process(
                    new MatchingContext(notifications, linkResult.Movements), Context.CancellationToken);

                var decisionContext = new DecisionContext(notifications, linkResult.Movements, matchResult, messageId);
                var decisionResult = await decisionService.Process(decisionContext, Context.CancellationToken);

                await validationService.PostDecision(linkResult, decisionResult, Context.CancellationToken);
            }
            else if (preProcessingResult.IsDeleted())
            {
                var linkContext = new ImportNotificationLinkContext(preProcessingResult.Record,
                    preProcessingResult.ChangeSet);
                await linkingService.UnLink(linkContext, Context.CancellationToken);

            }
            else
            {
                logger.LogWarning("Skipping Linking/Matching/Decisions for {Id} with MessageId {MessageId} with Pre-Processing Outcome {PreProcessingOutcome} Because Last AuditState was {AuditState}", message.ReferenceNumber, messageId, preProcessingResult.Outcome.ToString(), preProcessingResult.Record.GetLatestAuditEntry().Status);
                LogStatus("IsCreatedOrChanged=false", message);
            }

            await dbContext.SaveChangesAsync(Context.CancellationToken);

        }
    }

    private async Task<List<Model.Ipaffs.ImportNotification>> LoadAllNotificationReferenced(CancellationToken cancellationToken, LinkResult linkResult)
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
        logger.LogInformation("{state} : {id}, {version}, {lastUpdated}", state, message.ReferenceNumber, message.Version, message.LastUpdated);
    }

    public IConsumerContext Context { get; set; } = null!;
}
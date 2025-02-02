using Btms.Backend.Data;
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
using Microsoft.EntityFrameworkCore;
using DecisionContext = Btms.Business.Services.Decisions.DecisionContext;

namespace Btms.Consumers;

    internal class NotificationConsumer(IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification> preProcessor, ILinkingService linkingService,
        IMatchingService matchingService,
        IDecisionService decisionService,
        IValidationService validationService,
        ILogger<NotificationConsumer> logger,
        IMongoDbContext dbContext)
    : IConsumer<ImportNotification>, IConsumerWithContext
{
    public async Task OnHandle(ImportNotification message)
    {
        var messageId = Context.GetMessageId();
        using (logger.BeginScope(Context.GetJobId()!, messageId, GetType().Name, message.ReferenceNumber!))
        {
            var preProcessingResult = await preProcessor
                .Process(new PreProcessingContext<ImportNotification>(message, messageId));

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

                var matchResult = await matchingService.Process(
                    new MatchingContext(linkResult.Notifications, linkResult.Movements), Context.CancellationToken);

                var decisionContext = new DecisionContext(linkResult.Notifications, linkResult.Movements, matchResult);
                var decisionResult = await decisionService.Process(decisionContext, Context.CancellationToken);
                
                await validationService.PostDecision(linkResult, decisionResult, Context.CancellationToken);

                await dbContext.SaveChangesAsync(Context.CancellationToken);

                await Context.Bus.PublishDecisions(messageId, decisionResult, decisionContext);
            }
            else if (preProcessingResult.IsDeleted())
            {
                var linkContext = new ImportNotificationLinkContext(preProcessingResult.Record,
                    preProcessingResult.ChangeSet);
                await linkingService.UnLink(linkContext, Context.CancellationToken);
                await dbContext.SaveChangesAsync(Context.CancellationToken);

            }
            else
            {
                await dbContext.SaveChangesAsync(Context.CancellationToken);
                logger.LogWarning("Skipping Linking/Matching/Decisions for {Id} with MessageId {MessageId} with Pre-Processing Outcome {PreProcessingOutcome} Because Last AuditState was {AuditState}", message.ReferenceNumber, messageId, preProcessingResult.Outcome.ToString(), preProcessingResult.Record.GetLatestAuditEntry().Status);
                LogStatus("IsCreatedOrChanged=false", message);
            }

        }
    }

    private void LogStatus(string state, ImportNotification message)
    {
        logger.LogInformation("{state} : {id}, {version}, {lastUpdated}", state, message.ReferenceNumber, message.Version, message.LastUpdated);
    }

    public IConsumerContext Context { get; set; } = null!;
}
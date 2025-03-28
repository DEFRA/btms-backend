using Btms.Backend.Data;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Business.Services.Matching;
using Btms.Business.Services.Validating;
using Btms.Consumers.Extensions;
using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Consumers;

internal class ClearanceRequestConsumer(
    IPreProcessor<AlvsClearanceRequest, Model.Movement> preProcessor,
    ILinkingService linkingService,
    IMatchingService matchingService,
    IDecisionService decisionService,
    IValidationService validationService,
    IMongoDbContext dbContext,
    ILogger<ClearanceRequestConsumer> logger)
    : IConsumer<AlvsClearanceRequest>, IConsumerWithContext
{
    public async Task OnHandle(AlvsClearanceRequest message, CancellationToken cancellationToken)
    {
        var messageId = Context.GetMessageId();
        using (logger.BeginScope(Context.GetJobId()!, messageId, GetType().Name, message.Header?.EntryReference!))
        {
            var preProcessingResult = await preProcessor
                .Process(new PreProcessingContext<AlvsClearanceRequest>(message, messageId));

            if (preProcessingResult.Outcome == PreProcessingOutcome.ValidationError)
            {
                await dbContext.SaveChangesAsync(Context.CancellationToken);
                return;
            }
            else if (preProcessingResult.Outcome == PreProcessingOutcome.Skipped)
            {
                Context.Skipped();
            }
            else
            {
                Context.PreProcessed();
            }

            if (preProcessingResult.IsCreatedOrChanged())
            {
                var linkContext = new MovementLinkContext(preProcessingResult.Record,
                    preProcessingResult.ChangeSet);
                var linkResult = await linkingService.Link(linkContext, Context.CancellationToken);

                if (linkResult.Outcome != LinkOutcome.NotLinked)
                {
                    Context.Linked();
                }

                if (linkResult.IsAllNotificationsDeleted())
                {
                    await dbContext.SaveChangesAsync(Context.CancellationToken);
                    return;
                }

                if (!await validationService.PostLinking(linkContext, linkResult,
                        triggeringMovement: preProcessingResult.Record,
                        cancellationToken: Context.CancellationToken))
                {
                    logger.LogWarning(
                        "Skipping Matching/Decisions due to PostLinking failure for {Mrn} with MessageId {MessageId}",
                        message.Header?.EntryReference, messageId);
                    await dbContext.SaveChangesAsync(Context.CancellationToken);
                    return;
                }

                // We need to mark the entity as updated even if the conceptual resource has not changed
                // so that consumers of BTMS can query notifications where related data has changed but
                // the resource itself hasn't
                await dbContext.Notifications.Update(linkResult.Notifications, Context.CancellationToken);

                var matchResult = await matchingService.Process(
                    new MatchingContext(linkResult.Notifications, linkResult.Movements), Context.CancellationToken);

                var decisionContext =
                    new DecisionContext(linkResult.Notifications, linkResult.Movements, matchResult, messageId);
                var decisionResult = await decisionService.Process(decisionContext, Context.CancellationToken);

                await validationService.PostDecision(decisionContext, decisionResult, Context.CancellationToken);

                // Recalculate the status of the notifications before saving
                linkResult.Notifications.ForEach(n => n.CalculateStatus());
            }
            else
            {
                logger.LogWarning(
                    "Skipping Linking/Matching/Decisions for {Mrn} with MessageId {MessageId} with Pre-Processing Outcome {PreProcessingOutcome} Because Last AuditState was {AuditState}",
                    message.Header?.EntryReference, messageId, preProcessingResult.Outcome.ToString(),
                    preProcessingResult.Record.GetLatestAuditEntry().Status);
            }

            await dbContext.SaveChangesAsync(Context.CancellationToken);
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
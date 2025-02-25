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

public interface IClearanceRequestConsumer
{
    Task OnHandle(AlvsClearanceRequest message, IConsumerContext context, CancellationToken cancellationToken);
}

internal class ClearanceRequestConsumer(
    IPreProcessor<AlvsClearanceRequest, Model.Movement> preProcessor,
    ILinkingService linkingService,
    IMatchingService matchingService,
    IDecisionService decisionService,
    IValidationService validationService,
    IMongoDbContext dbContext,
    ILogger<ClearanceRequestConsumer> logger) : IClearanceRequestConsumer
{
    public async Task OnHandle(AlvsClearanceRequest message, IConsumerContext context, CancellationToken cancellationToken)
    {
        var messageId = context.GetMessageId();
        using (logger.BeginScope(context.GetJobId()!, messageId, GetType().Name, message.Header?.EntryReference!))
        {
            var preProcessingResult = await preProcessor
                .Process(new PreProcessingContext<AlvsClearanceRequest>(message, messageId));

            if (preProcessingResult.Outcome == PreProcessingOutcome.Skipped)
            {
                context.Skipped();
            }
            else
            {
                context.PreProcessed();
            }

            if (preProcessingResult.IsCreatedOrChanged())
            {
                var linkContext = new MovementLinkContext(preProcessingResult.Record,
                    preProcessingResult.ChangeSet);
                var linkResult = await linkingService.Link(linkContext, context.CancellationToken);

                if (linkResult.Outcome != LinkOutcome.NotLinked)
                {
                    context.Linked();
                }

                if (!await validationService.PostLinking(linkContext, linkResult,
                        triggeringMovement: preProcessingResult.Record,
                        cancellationToken: context.CancellationToken))
                {
                    logger.LogWarning(
                        "Skipping Matching/Decisions due to PostLinking failure for {Mrn} with MessageId {MessageId}",
                        message.Header?.EntryReference, messageId);
                    await dbContext.SaveChangesAsync(context.CancellationToken);
                    return;
                }

                // We need to mark the entity as updated even if the conceptual resource has not changed
                // so that consumers of BTMS can query notifications where related data has changed but
                // the resource itself hasn't
                await dbContext.Notifications.Update(linkResult.Notifications, context.CancellationToken);

                var matchResult = await matchingService.Process(
                    new MatchingContext(linkResult.Notifications, linkResult.Movements), context.CancellationToken);

                var decisionContext =
                    new DecisionContext(linkResult.Notifications, linkResult.Movements, matchResult, messageId);
                var decisionResult = await decisionService.Process(decisionContext, context.CancellationToken);

                await validationService.PostDecision(linkResult, decisionResult, context.CancellationToken);


            }
            else
            {
                logger.LogWarning(
                    "Skipping Linking/Matching/Decisions for {Mrn} with MessageId {MessageId} with Pre-Processing Outcome {PreProcessingOutcome} Because Last AuditState was {AuditState}",
                    message.Header?.EntryReference, messageId, preProcessingResult.Outcome.ToString(),
                    preProcessingResult.Record.GetLatestAuditEntry().Status);
            }

            await dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}
using Btms.Backend.Data;
using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using Btms.Consumers.Extensions;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Business.Services.Matching;
using Btms.Business.Services.Validating;
using Btms.Model;
using Btms.Model.Cds;
using DecisionContext = Btms.Business.Services.Decisions.DecisionContext;

namespace Btms.Consumers;

    internal class AlvsClearanceRequestConsumer(IPreProcessor<AlvsClearanceRequest, Model.Movement> preProcessor, ILinkingService linkingService,
        IMatchingService matchingService,
        IDecisionService decisionService, 
        IValidationService validationService,
        ILogger<AlvsClearanceRequestConsumer> logger,
        IMongoDbContext dbContext)
    : IConsumer<AlvsClearanceRequest>, IConsumerWithContext
{
    public async Task OnHandle(AlvsClearanceRequest message)
    {
        var messageId = Context.GetMessageId();
        using (logger.BeginScope(Context.GetJobId()!, messageId, GetType().Name, message.Header?.EntryReference!))
        {
            var preProcessingResult = await preProcessor
                .Process(new PreProcessingContext<AlvsClearanceRequest>(message, messageId));

            if (preProcessingResult.Outcome == PreProcessingOutcome.Skipped)
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

                // var m = new Movement()
                // {
                //     BtmsStatus = MovementStatus.Default()
                // };
                // (Movement)preProcessingResult.Record
                // 
                if (! await validationService.PostLinking(linkContext, linkResult, 
                        triggeringMovement: preProcessingResult.Record,
                        cancellationToken: Context.CancellationToken))
                {
                    logger.LogWarning("Skipping Matching/Decisions due to PostLinking failure for {Mrn} with MessageId {MessageId}", message.Header?.EntryReference, messageId);
                    await dbContext.SaveChangesAsync(Context.CancellationToken);
                    return;
                }

               

                var matchResult = await matchingService.Process(
                    new MatchingContext(linkResult.Notifications, linkResult.Movements), Context.CancellationToken);

                var decisionContext = new DecisionContext(linkResult.Notifications, linkResult.Movements, matchResult, true);
                var decisionResult = await decisionService.Process(decisionContext, Context.CancellationToken);
                
                await validationService.PostDecision(linkResult, decisionResult, Context.CancellationToken);

                await dbContext.SaveChangesAsync(Context.CancellationToken);

                foreach (var decisionMessage in decisionResult.DecisionsMessages)
                {
                    var headers = new Dictionary<string, object>
                    {
                        { "messageId", Guid.NewGuid() },
                        { "notifications", decisionContext.Notifications
                            .Select(n => new DecisionImportNotifications
                            {
                                Id = n.Id!,
                                Version = n.Version,
                                Created = n.Created,
                                Updated = n.Updated,
                                UpdatedEntity = n.UpdatedEntity,
                                CreatedSource = n.CreatedSource!.Value,
                                UpdatedSource = n.UpdatedSource!.Value
                            })
                            .ToList()
                        },
                    };
                    await Context.Bus.Publish(decisionMessage, "DECISIONS", headers: headers, cancellationToken: Context.CancellationToken);
                }
            }
            else
            {
                logger.LogWarning("Skipping Linking/Matching/Decisions for {Mrn} with MessageId {MessageId} Because Last AuditState was {AuditState}", message.Header?.EntryReference, messageId, preProcessingResult.Record.GetLatestAuditEntry().Status);
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
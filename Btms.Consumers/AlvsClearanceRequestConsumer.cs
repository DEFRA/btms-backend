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
using Btms.Model.Auditing;
using Btms.Model.Cds;
using DecisionContext = Btms.Business.Services.Decisions.DecisionContext;

namespace Btms.Consumers;

    internal class AlvsClearanceRequestConsumer(IPreProcessor<AlvsClearanceRequest, Model.Movement> preProcessor, ILinkingService linkingService,
        IMatchingService matchingService,
        IDecisionService decisionService, 
        IValidationService validationService,
        IAssociatedDataService associatedDataService,
        ILogger<AlvsClearanceRequestConsumer> logger)
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
                    return;
                }
                
                await associatedDataService.Update(linkResult.Notifications, messageId, Context.CancellationToken);

                var matchResult = await matchingService.Process(
                    new MatchingContext(linkResult.Notifications, linkResult.Movements), Context.CancellationToken);

                var decisionResult = await decisionService.Process(new DecisionContext(linkResult.Notifications, linkResult.Movements, matchResult, true), Context.CancellationToken);
                
                await validationService.PostDecision(linkResult, decisionResult, Context.CancellationToken);
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
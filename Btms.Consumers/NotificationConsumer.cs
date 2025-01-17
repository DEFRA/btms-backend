using Btms.Types.Ipaffs;
using SlimMessageBus;
using Btms.Consumers.Extensions;
using Microsoft.Extensions.Logging;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Business.Services.Matching;
using Btms.Business.Services.Validating;

namespace Btms.Consumers;

    internal class NotificationConsumer(IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification> preProcessor, ILinkingService linkingService,
        IMatchingService matchingService,
        IDecisionService decisionService,
        IValidationService validationService,
        ILogger<NotificationConsumer> logger)
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
                    return;
                }

                var matchResult = await matchingService.Process(
                    new MatchingContext(linkResult.Notifications, linkResult.Movements), Context.CancellationToken);

                var decisionResult = await decisionService.Process(new DecisionContext(linkResult.Notifications, linkResult.Movements, matchResult), Context.CancellationToken);
                
                await validationService.PostDecision(linkResult, decisionResult, Context.CancellationToken);
            }
            else
            {
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
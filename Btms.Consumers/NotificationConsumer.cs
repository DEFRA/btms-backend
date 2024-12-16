using Btms.Types.Ipaffs;
using SlimMessageBus;
using Btms.Consumers.Extensions;
using Microsoft.Extensions.Logging;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Business.Services.Matching;

namespace Btms.Consumers
{
    internal class NotificationConsumer(IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification> preProcessor, ILinkingService linkingService,
        IMatchingService matchingService,
        IDecisionService decisionService,
        ILogger<NotificationConsumer> logger)
        : IConsumer<ImportNotification>, IConsumerWithContext
    {
        public async Task OnHandle(ImportNotification message)
        {
            var messageId = Context.GetMessageId();
            using (logger.BeginScope(Context.GetJobId()!, messageId, GetType().Name, message.ReferenceNumber!))
            {
                var preProcessingResult = await preProcessor.Process(new PreProcessingContext<ImportNotification>(message, messageId));

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
                    var linkContext = new ImportNotificationLinkContext(preProcessingResult.Record!,
                        preProcessingResult.ChangeSet);
                    var linkResult = await linkingService.Link(linkContext, Context.CancellationToken);

                    if (linkResult.Outcome == LinkOutcome.Linked)
                    {
                        Context.Linked();
                    }

                    var matchResult = await matchingService.Process(
                        new MatchingContext(linkResult.Notifications, linkResult.Movements), Context.CancellationToken);

                    await decisionService.Process(new DecisionContext(linkResult.Notifications, linkResult.Movements, matchResult), Context.CancellationToken);
                }

            }
        }

        public IConsumerContext Context { get; set; } = null!;
    }
}
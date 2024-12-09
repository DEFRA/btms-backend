using Btms.Business.Services;
using Btms.Types.Ipaffs;
using SlimMessageBus;
using Btms.Consumers.Extensions;
using Microsoft.Extensions.Logging;
using Btms.Business.Pipelines.PreProcessing;

namespace Btms.Consumers
{
    internal class NotificationConsumer(IPreProcessor<ImportNotification, Model.Ipaffs.ImportNotification> preProcessor, ILinkingService linkingService, ILogger<NotificationConsumer> logger)
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
                }

            }
        }

        public IConsumerContext Context { get; set; } = null!;
    }
}
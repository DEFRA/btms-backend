using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using Btms.Consumers.Extensions;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Linking;

namespace Btms.Consumers;

internal class AlvsClearanceRequestConsumer(IPreProcessor<AlvsClearanceRequest, Model.Movement> preProcessor, ILinkingService linkingService, ILogger<AlvsClearanceRequestConsumer> logger)
    : IConsumer<AlvsClearanceRequest>, IConsumerWithContext
{
    public async Task OnHandle(AlvsClearanceRequest message)
    {
        var messageId = Context.GetMessageId();
        using (logger.BeginScope(Context.GetJobId()!, messageId, GetType().Name, message.Header?.EntryReference!))
        {
            var preProcessingResult = await preProcessor.Process(new PreProcessingContext<AlvsClearanceRequest>(message, messageId));

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

                if (linkResult.Outcome == LinkOutcome.Linked)
                {
                    Context.Linked();
                }
            }
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
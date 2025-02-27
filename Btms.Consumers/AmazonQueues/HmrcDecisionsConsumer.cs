using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Consumers.AmazonQueues;

internal class HmrcDecisionsConsumer(IDecisionsConsumer decisionsConsumer, ILogger<HmrcDecisionsConsumer> logger) : MessageConsumer<Decision>(logger)
{
    protected override async Task OnHandle(Decision message, IConsumerContext context, CancellationToken cancellationToken)
    {
        await decisionsConsumer.OnHandle(message, Context, cancellationToken);
    }
}
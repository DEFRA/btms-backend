using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Consumers.AmazonQueues;

internal class HmrcClearanceRequestConsumer(IClearanceRequestConsumer clearanceRequestConsumer, ILogger<HmrcClearanceRequestConsumer> logger) : MessageConsumer<AlvsClearanceRequest>(logger)
{
    protected override async Task OnHandle(AlvsClearanceRequest message, IConsumerContext context, CancellationToken cancellationToken)
    {
        await clearanceRequestConsumer.OnHandle(message, Context, cancellationToken);
    }
}
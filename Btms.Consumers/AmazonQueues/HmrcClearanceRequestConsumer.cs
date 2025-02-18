using Btms.Types.Alvs;
using SlimMessageBus;

namespace Btms.Consumers.AmazonQueues;

internal class HmrcClearanceRequestConsumer(ClearanceRequestConsumer clearanceRequestConsumer) : MessageConsumer<AlvsClearanceRequest>
{
    protected override async Task OnHandle(AlvsClearanceRequest message, IConsumerContext context, CancellationToken cancellationToken)
    {
        await clearanceRequestConsumer.OnHandle(message, Context, cancellationToken);
    }
}

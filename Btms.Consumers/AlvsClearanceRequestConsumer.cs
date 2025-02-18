using Btms.Types.Alvs;
using SlimMessageBus;

namespace Btms.Consumers;

internal class AlvsClearanceRequestConsumer(ClearanceRequestConsumer clearanceRequestConsumer)
    : IConsumer<AlvsClearanceRequest>, IConsumerWithContext
{
    public async Task OnHandle(AlvsClearanceRequest message, CancellationToken cancellationToken)
    {
        await clearanceRequestConsumer.OnHandle(message, Context, cancellationToken);
    }

    public IConsumerContext Context { get; set; } = null!;
}
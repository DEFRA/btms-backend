using Btms.Types.Alvs;
using SlimMessageBus;

namespace Btms.Consumers;

internal class AlvsDecisionsConsumer(IDecisionsConsumer decisionsConsumer)
    : IConsumer<Decision>, IConsumerWithContext
{
    public async Task OnHandle(Decision message, CancellationToken cancellationToken)
    {
        await decisionsConsumer.OnHandle(message, Context, cancellationToken);
    }

    public IConsumerContext Context { get; set; } = null!;
}
using Btms.Consumers.Extensions;
using Btms.Types.Gvms;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Consumers;

internal class GmrAsbConsumer(ILogger<GmrAsbConsumer> logger) : IConsumer<Gmr>, IConsumerWithContext
{
    public Task OnHandle(Gmr message, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Consumed {GmrId}, {MessageId}", message.GmrId, Context.GetMessageId());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Errored");
        }

        return Task.CompletedTask;
    }

    public IConsumerContext Context { get; set; } = null!;
}
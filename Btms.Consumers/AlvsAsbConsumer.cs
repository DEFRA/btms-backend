using Btms.Consumers.Extensions;
using Btms.Model.Cds;
using Btms.SensitiveData;
using Btms.Types.Alvs;
using Microsoft.Extensions.DependencyInjection;
using SlimMessageBus;

namespace Btms.Consumers;

internal class AlvsAsbConsumer(IServiceProvider serviceProvider) : IConsumer<object>, IConsumerWithContext
{
    private readonly ISensitiveDataSerializer _serializer = serviceProvider.GetRequiredService<ISensitiveDataSerializer>();
    public Task OnHandle(object message)
    {
        var messageType = Context.GetMessageType();

        return messageType switch
        {
            AlvsMessageTypes.ALVSClearanceRequest => ProcessMessage<AlvsClearanceRequest>(message),
            AlvsMessageTypes.ALVSDecisionNotification => ProcessMessage<Decision>(message),
            ////AlvsMessageTypes.FinalisationNotificationRequest => ProcessMessage<ImportNotification>(message),
            _ => Task.CompletedTask
        };
    }

    private Task ProcessMessage<T>(object message)
    {
        var newMessage = _serializer.Deserialize<T>(message.ToString() ?? throw new InvalidOperationException());
        var consumer = serviceProvider.GetRequiredService<IConsumer<T>>();
        if (consumer is IConsumerWithContext consumerWithContext)
        {
            consumerWithContext.Context = Context;
        }

        return consumer.OnHandle(newMessage);
    }

    public IConsumerContext Context { get; set; } = null!;
}
using SlimMessageBus;

namespace Btms.Consumers.ConsumerContextAccessor.ContextAccessor;

public interface IConsumerContextAccessor
{
    IConsumerContext? ConsumerContext { get; set; }
}
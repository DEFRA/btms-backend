using System.Diagnostics;
using Btms.Consumers.ConsumerContextAccessor.ContextAccessor;
using SlimMessageBus;

namespace Btms.Consumers.ContextAccessor;

[DebuggerDisplay("ConsumerContext = {ConsumerContext}")]
public class ConsumerContextAccessor : IConsumerContextAccessor
{
    private static readonly AsyncLocal<ConsumerContextHolder> _currentContext = new AsyncLocal<ConsumerContextHolder>();

    /// <inheritdoc/>
    public IConsumerContext? ConsumerContext
    {
        get => _currentContext.Value?.Context;
        set
        {
            var holder = _currentContext.Value;
            if (holder != null)
            {
                // Clear current HttpContext trapped in the AsyncLocals, as its done.
                holder.Context = null;
            }

            if (value != null)
            {
                // Use an object indirection to hold the HttpContext in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when its cleared.
                _currentContext.Value = new ConsumerContextHolder { Context = value };
            }
        }
    }

    private sealed class ConsumerContextHolder
    {
        public IConsumerContext? Context;
    }
}
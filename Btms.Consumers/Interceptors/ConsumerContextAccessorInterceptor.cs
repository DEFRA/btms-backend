using Btms.Consumers.ConsumerContextAccessor.ContextAccessor;
using Microsoft.Extensions.Configuration;
using Serilog.Context;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Btms.Consumers.Interceptors;

public class ConsumerContextAccessorInterceptor<TMessage>(IConfiguration configuration) : IConsumerInterceptor<TMessage>
{
    public async Task<object> OnHandle(TMessage message, Func<Task<object>> next, IConsumerContext context)
    {
        var traceIdHeader = configuration.GetValue<string>("TraceHeader");

        if (traceIdHeader == null)
        {
            return await next();
        }

        ////consumerContextAccessor.ConsumerContext = context;

        context.Headers.TryGetValue(traceIdHeader, out object? headerValue);
        var requestHeader = headerValue?.ToString();

        if (!string.IsNullOrWhiteSpace(requestHeader))
        {
            using (LogContext.PushProperty("CorrelationId", requestHeader))
            {
                return await next();
            }
        }

        return await next();
    }
}
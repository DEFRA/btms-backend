using Btms.Consumers.ConsumerContextAccessor.ContextAccessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;
using System.Collections.Generic;

namespace Btms.Consumers.Interceptors;

public class ConsumerContextAccessorInterceptor<TMessage>(IConfiguration configuration, ILogger<ConsumerContextAccessorInterceptor<TMessage>> logger) : IConsumerInterceptor<TMessage>
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
                logger.LogInformation("Set CorrelationId to {CorrelationId}", requestHeader);
                return await next();
            }
        }

        logger.LogInformation("No CorrelationId found on Message Header to {Headers}", string.Join(Environment.NewLine, context.Headers));

        return await next();
    }
}
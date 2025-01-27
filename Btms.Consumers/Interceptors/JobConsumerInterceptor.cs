using Btms.Consumers.Extensions;
using Btms.SyncJob;
using Microsoft.Extensions.Options;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Btms.Consumers.Interceptors;


public class JobConsumerInterceptor<TMessage>(ISyncJobStore store, IOptions<ConsumerOptions> options) : IConsumerInterceptor<TMessage>
{
    public async Task<object> OnHandle(TMessage message, Func<Task<object>> next, IConsumerContext context)
    {
        if (!context.Headers.TryGetValue("jobId", out var value))
        {
            return await next();
        }

        var job = store.GetJob(Guid.Parse(value.ToString()!));
        try
        {
            var result = await next();
            job?.MessageProcessed();
            return result;
        }
        catch (Exception)
        {
            if (context.GetRetryAttempt() > options.Value.ErrorRetries)
            {
                job?.MessageFailed();
            }

            throw;
        }
    }
}
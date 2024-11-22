using Cdms.Consumers.Extensions;
using Cdms.SyncJob;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Cdms.Consumers.Interceptors;


public class JobConsumerInterceptor<TMessage>(ISyncJobStore store) : IConsumerInterceptor<TMessage>
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
            if (context.GetRetryAttempt() == 5)
            {
                job?.MessageFailed();
            }

            throw;
        }
    }
}
using AsyncKeyedLock;
using Btms.Consumers.Extensions;
using Btms.Types.Ipaffs;
using SlimMessageBus;
using SlimMessageBus.Host.Interceptor;

namespace Btms.Consumers.Interceptors;

public class NotificationLockInterceptor : IConsumerInterceptor<ImportNotification>
{
    private static readonly AsyncKeyedLocker<string> _asyncKeyedLocker = new();

    public async Task<object> OnHandle(ImportNotification message, Func<Task<object>> next, IConsumerContext context)
    {
        if (context.UseLock())
        {
            using var releaser = await _asyncKeyedLocker.LockAsync(message.ReferenceNumber!, context.CancellationToken);
            return next();
        }

        return next();
    }
}
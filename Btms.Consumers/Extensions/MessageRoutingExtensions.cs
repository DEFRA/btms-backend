using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using SlimMessageBus.Host;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Consumers.Extensions;

/// <summary>
/// These are used by other libraries - expect testing, to push lists of messages directly into the consumers
/// </summary>
public static class MessageRoutingExtensions
{
    private const string MESSAGE_ID_HEADER_KEY = "messageId";
    
    public static async Task PushToConsumers(this IServiceProvider sp, 
        ILogger logger, IEnumerable<object> messages,
        int sleepMs = 1000, bool synchronous = false)
    {
        var output = new List<object>();
        await PushMessagesToConsumers(sp, logger, messages
                .Where(m => m is AlvsClearanceRequest or ImportNotification),
            sleepMs, synchronous);

        await PushMessagesToConsumers(sp, logger, messages
                .Where(m => m is Decision),
            sleepMs, synchronous);
        
        await PushMessagesToConsumers(sp, logger, messages
                .Where(m => m is Finalisation),
            sleepMs, synchronous);
    }
    public static async Task PushMessagesToConsumers(this IServiceProvider sp, 
        ILogger logger, IEnumerable<object> messages, 
        int sleepMs = 1000, bool synchronous = false)
    {
        var output = new List<object>();
        
        foreach (var message in messages)
        {
            var scope = sp.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IPublishBus>();
            var headers = new Dictionary<string, object>();
            switch (message)
            {
                case null:
                    throw new ArgumentNullException();

                case ImportNotification n:
                    headers.Add(MESSAGE_ID_HEADER_KEY, n.ReferenceNumber!);
                    await bus.Publish(n, "NOTIFICATIONS", headers);
                    logger.LogInformation("Sent notification {0} to consumer", n.ReferenceNumber!);
                    break;

                case AlvsClearanceRequest cr:
                    headers.Add(MESSAGE_ID_HEADER_KEY, cr.Header!.EntryReference!);
                    await bus.Publish(cr, "CLEARANCEREQUESTS", headers);
                    logger.LogInformation("Sent cr {0} to consumer", cr.Header!.EntryReference!);
                    break;

                case Decision d:
                    headers.Add(MESSAGE_ID_HEADER_KEY, d.Header!.EntryReference!);
                    await bus.Publish(d, "DECISIONS", headers);
                    logger.LogInformation("Sent decision {0} to consumer", d.Header!.EntryReference!);
                    break;

                case Finalisation d:
                    headers.Add(MESSAGE_ID_HEADER_KEY, d.Header!.EntryReference!);
                    await bus.Publish(d, "FINALISATIONS", headers);
                    logger.LogInformation("Sent finalisation {0} to consumer", d.Header!.EntryReference!);
                    break;

                default:
                    throw new ArgumentException($"Unexpected type {message.GetType().Name}");
            }
        }

        await Task.Delay(1000);
    }
}
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
    public static async Task PushToConsumers(this IServiceProvider sp, ILogger logger, IEnumerable<object> messages, int sleepMs = 1000)
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
                    headers.Add("messageId", n.ReferenceNumber!);
                    await bus.Publish(n, "NOTIFICATIONS", headers);
                    logger.LogInformation("Sent notification {0} to consumer", n.ReferenceNumber!);
                    break;

                case Decision d:
                    headers.Add("messageId", d.Header!.EntryReference!);
                    await bus.Publish(d, "DECISIONS", headers);
                    logger.LogInformation("Sent decision {0} to consumer", d.Header!.EntryReference!);
                    break;

                case AlvsClearanceRequest cr:
                    headers.Add("messageId", cr.Header!.EntryReference!);
                    await bus.Publish(cr, "CLEARANCEREQUESTS", headers);
                    logger.LogInformation("Sent cr {0} to consumer", cr.Header!.EntryReference!);
                    break;

                default:
                    throw new ArgumentException($"Unexpected type {message.GetType().Name}");
            }
        }
    }
}
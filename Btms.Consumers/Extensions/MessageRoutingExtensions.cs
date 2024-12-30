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
    public static async Task PushToConsumers(this IServiceProvider sp, ILogger logger, IEnumerable<object> messages)
    {
        var output = new List<object>();
        
        foreach (var message in messages)
        {
            var scope = sp.CreateScope();

            switch (message)
            {
                case null:
                    throw new ArgumentNullException();

                case ImportNotification n:

                    var notificationConsumer = (NotificationConsumer)scope
                        .ServiceProvider
                        .GetRequiredService<IConsumer<ImportNotification>>();

                    notificationConsumer.Context = new ConsumerContext
                    {
                        Headers = new Dictionary<string, object> { { "messageId", n.ReferenceNumber! } }
                    };

                    await notificationConsumer.OnHandle(n);
                    logger.LogInformation("Sent notification {0} to consumer", n.ReferenceNumber!);
                    break;

                case Decision d:

                    var decisionConsumer = (DecisionsConsumer)scope
                        .ServiceProvider
                        .GetRequiredService<IConsumer<Decision>>();

                    decisionConsumer.Context = new ConsumerContext
                    {
                        Headers = new Dictionary<string, object> { { "messageId", d.Header!.EntryReference! } }
                    };

                    await decisionConsumer.OnHandle(d);
                    logger.LogInformation("Sent decision {0} to consumer", d.Header!.EntryReference!);
                    break;

                case AlvsClearanceRequest cr:

                    var crConsumer = (AlvsClearanceRequestConsumer)scope
                        .ServiceProvider
                        .GetRequiredService<IConsumer<AlvsClearanceRequest>>();

                    crConsumer.Context = new ConsumerContext
                    {
                        Headers = new Dictionary<string, object> { { "messageId", cr.Header!.EntryReference! } }
                    };

                    await crConsumer.OnHandle(cr);
                    logger.LogInformation("Sent cr {0} to consumer", cr.Header!.EntryReference!);
                    break;

                default:
                    throw new ArgumentException($"Unexpected type {message.GetType().Name}");
            }
            
            // This sleep is to allow the system to settle, and have made any links & decisions
            // Before sending in the next message and potentially causing a concurrency issue
            // We may want to switch to pushing to the bus, rather than directly to the consumer
            // so we get the concurrency protection.

            Thread.Sleep(1000);
        }
    }
}
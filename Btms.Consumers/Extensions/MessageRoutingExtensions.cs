using Btms.Types.Alvs;
using Btms.Types.Gvms;
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
    private const string MessageIdHeaderKey = "messageId";

    public static async Task PushToConsumers(this IServiceProvider sp,
        ILogger logger, IEnumerable<object> messages,
        bool maintainOrder = false)
    {
        if (maintainOrder)
        {
            // Allow the data generator to control the ordering
            await PushMessagesToConsumers(sp, logger, messages);
        }
        else
        {
            await PushMessagesToConsumers(sp, logger, messages
                    .Where(m => m is AlvsClearanceRequest or ImportNotification));

            await PushMessagesToConsumers(sp, logger, messages
                    .Where(m => m is Decision));

            await PushMessagesToConsumers(sp, logger, messages
                    .Where(m => m is Finalisation));

            await PushMessagesToConsumers(sp, logger, messages
                .Where(m => m is SearchGmrsForDeclarationIdsResponse));
        }

    }

    private static async Task PushMessagesToConsumers(this IServiceProvider sp,
        ILogger logger, IEnumerable<object> messages)
    {

        foreach (var message in messages)
        {
            var scope = sp.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IPublishBus>();
            var headers = new Dictionary<string, object>();
            switch (message)
            {
                case null:
                    throw new ArgumentException($"Unexpected null message");

                case ImportNotification n:
                    headers.Add(MessageIdHeaderKey, n.ReferenceNumber!);
                    await bus.Publish(n, "NOTIFICATIONS", headers);
                    logger.LogInformation("Sent notification {0} to consumer", n.ReferenceNumber!);
                    break;

                case AlvsClearanceRequest cr:
                    headers.Add(MessageIdHeaderKey, cr.Header!.EntryReference!);
                    await bus.Publish(cr, "CLEARANCEREQUESTS", headers);
                    logger.LogInformation("Sent cr {0} to consumer", cr.Header!.EntryReference!);
                    break;

                case Decision d:
                    headers.Add(MessageIdHeaderKey, d.Header!.EntryReference!);
                    await bus.Publish(d, "DECISIONS", headers);
                    logger.LogInformation("Sent decision {0} to consumer", d.Header!.EntryReference!);
                    break;

                case Finalisation d:
                    headers.Add(MessageIdHeaderKey, d.Header!.EntryReference!);
                    await bus.Publish(d, "FINALISATIONS", headers);
                    logger.LogInformation("Sent finalisation {0} to consumer", d.Header!.EntryReference!);
                    break;

                case Gmr d:
                    headers.Add(MessageIdHeaderKey, d.GmrId!);
                    await bus.Publish(d, "GMR", headers);
                    logger.LogInformation("Sent gmr {0} to consumer", d.GmrId!);
                    break;

                case SearchGmrsForDeclarationIdsResponse d:
                    var messageId = Guid.NewGuid().ToString();
                    headers.Add(MessageIdHeaderKey, messageId);
                    await bus.Publish(d, "GMR", headers);
                    logger.LogInformation("Sent GMR search response {0} to consumer", messageId);
                    break;

                default:
                    throw new ArgumentException($"Unexpected type {message.GetType().Name}");
            }
        }
    }
}
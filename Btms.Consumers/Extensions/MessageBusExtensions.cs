using Btms.Business.Services.Decisions;
using Btms.Common.Extensions;
using Btms.Model.Cds;
using Btms.Model.Extensions;
using SlimMessageBus;
using DecisionContext = Btms.Business.Services.Decisions.DecisionContext;

namespace Btms.Consumers.Extensions;

public static class MessageBusExtensions
{
    public static async Task PublishDecisions(this IMessageBus bus, string messageId, DecisionResult decisionResult, DecisionContext decisionContext, CancellationToken cancellationToken = default)
    {
        foreach (var decisionMessage in decisionResult.DecisionsMessages)
        {
            var headers = new Dictionary<string, object>
            {
                { "messageId", messageId },
                { "notifications", decisionContext.Notifications
                    .Select(n => n.AsDecisionImportNotification())
                    .ToList()
                },
            };

            await bus.Publish(decisionMessage, "DECISIONS", headers: headers, cancellationToken: cancellationToken);
        }
    }
}
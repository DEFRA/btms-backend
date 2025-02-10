using Btms.Business.Services.Decisions;
using Btms.Model.Cds;
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
                    .Select(n => new DecisionImportNotifications
                    {
                        Id = n.Id!,
                        Version = n.Version,
                        Created = n.Created,
                        Updated = n.Updated,
                        UpdatedEntity = n.UpdatedEntity,
                        CreatedSource = n.CreatedSource!.Value,
                        UpdatedSource = n.UpdatedSource!.Value
                    })
                    .ToList()
                },
            };
            
            await bus.Publish(decisionMessage, "DECISIONS", headers: headers, cancellationToken: cancellationToken);
        }
    }
}
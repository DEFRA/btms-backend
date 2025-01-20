using Btms.Business.Services.Decisions.Finders;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Business.Services.Decisions;

public class DecisionService(ILogger<DecisionService> logger, IPublishBus bus, IEnumerable<IDecisionFinder> decisionFinders) : IDecisionService
{
    public async Task<DecisionResult> Process(DecisionContext decisionContext, CancellationToken cancellationToken)
    {
        var decisionResult = await DeriveDecision(decisionContext);

        var messages = await DecisionMessageBuilder.Build(decisionContext, decisionResult);

        foreach (var message in messages)
        {
            var headers = new Dictionary<string, object>
            {
                { "messageId", Guid.NewGuid() },
                { "notifications", decisionContext.Notifications
                    .Select(n => new DecisionImportNotifications
                    {
                        Id = n.Id!,
                        Version = n.Version,
                        Created = n.Created,
                        Updated = n.Updated,
                        CreatedSource = n.CreatedSource!.Value,
                        UpdatedSource = n.UpdatedSource!.Value
                        })
                    .ToList()
                },
            };
            await bus.Publish(message, "DECISIONS", headers: headers, cancellationToken: cancellationToken);
        }

        return decisionResult;
    }

    private Task<DecisionResult> DeriveDecision(DecisionContext decisionContext)
    {
        var decisionsResult = new DecisionResult();
        if (decisionContext.GenerateNoMatch)
        {
            foreach (var noMatch in decisionContext.MatchingResult.NoMatches)
            {
                if (decisionContext.HasChecks(noMatch.MovementId, noMatch.ItemNumber))
                {
                    decisionsResult.AddDecision(noMatch.MovementId, noMatch.ItemNumber, noMatch.DocumentReference, DecisionCode.X00);
                }
            }
        }

        foreach (var match in decisionContext.MatchingResult.Matches)
        {
            if (decisionContext.HasChecks(match.MovementId, match.ItemNumber))
            {
                var notification = decisionContext.Notifications.First(x => x.Id == match.NotificationId);
                var movement = decisionContext.Movements.First(x => x.Id == match.MovementId);
                var checkCodes = movement.Items.First(x => x.ItemNumber == match.ItemNumber).Checks?.Select(x => x.CheckCode).Where(x => x != null).Cast<string>().ToArray();
                var decisionCodes = GetDecisions(notification, checkCodes);
                foreach (var decisionCode in decisionCodes) 
                    decisionsResult.AddDecision(match.MovementId, match.ItemNumber, match.DocumentReference, decisionCode.DecisionCode, decisionCode.DecisionReason);
            }
        }

        return Task.FromResult(decisionsResult);
    }

    private DecisionFinderResult[] GetDecisions(ImportNotification notification, string[]? checkCodes)
    {
        var finders = decisionFinders.Where(x => x.CanFindDecision(notification, checkCodes)).ToArray();

        if (finders.Length == 0) throw new InvalidOperationException("Invalid ImportNotificationType / IUUCheckRequired");

        var results = finders.Select(x => x.FindDecision(notification, checkCodes)).ToArray();

        var item = 1;
        foreach (var result in results)
            logger.LogInformation("Decision finder result {ItemNum} of {NumItems} for Notification {Id} Decision {Decision} - ConsignmentAcceptable {ConsignmentAcceptable}: DecisionEnum {DecisionEnum}: NotAcceptableAction {NotAcceptableAction}",
                item++, results.Length, notification.Id, result.DecisionCode.ToString(), notification.PartTwo?.Decision?.ConsignmentAcceptable, notification.PartTwo?.Decision?.DecisionEnum.ToString(), notification.PartTwo?.Decision?.NotAcceptableAction?.ToString());
     
        return results;
    }
}
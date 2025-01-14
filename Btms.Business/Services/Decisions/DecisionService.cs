using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Business.Services.Decisions;

public class DecisionService(ILogger<DecisionService> logger, IPublishBus bus) : IDecisionService
{

    public async Task<DecisionResult> Process(DecisionContext decisionContext, CancellationToken cancellationToken)
    {
        var decisionResult = await DeriveDecision(decisionContext);

        var messages = await DecisionMessageBuilder.Build(decisionContext, decisionResult);

        var a = new { A = "B" };
        
        foreach (var message in messages)
        {
            var headers = new Dictionary<string, object>()
            {
                { "messageId", Guid.NewGuid() },
                { "notifications", decisionContext.Notifications!
                    .Select(n => new DecisionImportNotifications()
                    {
                        Id = n.Id!,
                        Version = n.Version,
                        Created = n.Created,
                        Updated = n.Updated,
                        CreatedSource = n.CreatedSource!.Value,
                        UpdatedSource = n.UpdatedSource!.Value!
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
                    decisionsResult.AddDecision(noMatch.MovementId, noMatch.ItemNumber, noMatch.DocumentReference,
                        DecisionCode.X00);
                }
            }
        }

        foreach (var match in decisionContext.MatchingResult.Matches)
        {
            if (decisionContext.HasChecks(match.MovementId, match.ItemNumber))
            {
                var n = decisionContext.Notifications.First(x => x.Id == match.NotificationId);
                var decisionCode = GetDecision(n);
                decisionsResult.AddDecision(match.MovementId, match.ItemNumber, match.DocumentReference,
                    decisionCode.DecisionCode);
            }
        }

        return Task.FromResult(decisionsResult);
    }
    

    private DecisionFinderResult GetDecision(ImportNotification notification)
    {
        // get decision finder - fold IUU stuff into the decision finder for fish?
        IDecisionFinder finder = notification.ImportNotificationType switch
        {
            ImportNotificationTypeEnum.Ced => new ChedDDecisionFinder(),
            ImportNotificationTypeEnum.Cveda => new ChedADecisionFinder(),
            ImportNotificationTypeEnum.Cvedp => new ChedPDecisionFinder(),
            ImportNotificationTypeEnum.Chedpp => new ChedPPDecisionFinder(),
            _ => throw new InvalidOperationException("Invalid ImportNotificationType")
        };

        var result =  finder.FindDecision(notification);
        logger.LogInformation("Decision finder result for Notification {Id} Decision {Decision} - ConsignmentAcceptable {ConsignmentAcceptable}: DecisionEnum {DecisionEnum}: NotAcceptableAction {NotAcceptableAction}",
            notification.Id, result.DecisionCode.ToString(), notification.PartTwo?.Decision?.ConsignmentAcceptable, notification.PartTwo?.Decision?.DecisionEnum.ToString(), notification.PartTwo?.Decision?.NotAcceptableAction?.ToString());
        return result;
    }


}
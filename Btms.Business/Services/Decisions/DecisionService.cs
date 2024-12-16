using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;
using SlimMessageBus;

namespace Btms.Business.Services.Decisions;

public class DecisionService(IPublishBus bus) : IDecisionService
{

    public async Task<DecisionResult> Process(DecisionContext decisionContext, CancellationToken cancellationToken)
    {
        var decisionResult = await DeriveDecision(decisionContext);

        var messages = await DecisionMessageBuilder.Build(decisionContext, decisionResult);

        foreach (var message in messages)
        {
            var headers = new Dictionary<string, object>()
            {
                { "messageId", Guid.NewGuid() }
            };
            await bus.Publish(message, "DECISIONS", headers: headers, cancellationToken: cancellationToken);
        }

        return decisionResult;
    }

    private static Task<DecisionResult> DeriveDecision(DecisionContext decisionContext)
    {
        var decisionsResult = new DecisionResult();
        if (decisionContext.GenerateNoMatch)
        {
            foreach (var noMatch in decisionContext.MatchingResult.NoMatches)
            {
                decisionsResult.AddDecision(noMatch.MovementId, noMatch.ItemNumber, noMatch.DocumentReference,
                    DecisionCode.X00);
            }
        }

        ////Not part of no matches, and the finders haven't been implemented yet, so leaving this commented out for the moment
        ////foreach (var match in decisionContext.MatchingResult.Matches)
        ////{
        ////    var n = decisionContext.Notifications.First(x => x.Id == match.NotificationId);
        ////    var decisionCode = GetDecision(n);
        ////    decisionsResult.AddDecision(match.MovementId, match.ItemNumber, match.DocumentReference, decisionCode.DecisionCode);
        ////}

        return Task.FromResult(decisionsResult);
    }

    
#pragma warning disable S1144
    private static DecisionFinderResult GetDecision(ImportNotification notification)
#pragma warning restore S1144
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

        return finder.FindDecision(notification);
    }


}
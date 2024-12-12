using System.Diagnostics;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model;
using Btms.Model.Ipaffs;
using SlimMessageBus;

namespace Btms.Business.Services.Decisions;

public class DecisionService(IDecisionMessageBuilder messageBuilder, IPublishBus bus) : IDecisionService
{

    public async Task<DecisionResult> Process(DecisionContext decisionContext, CancellationToken cancellationToken)
    {
        var decisionResult = await DeriveDecision(decisionContext);

        var messages = await messageBuilder.Build(decisionResult);

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
        var decisionResult = new DecisionResult();
        foreach (var movement in decisionContext.Movements)
        {
            var movementDecisionResult = decisionResult.AddDecision(movement.EntryReference, movement.EntryVersionNumber);
            
            foreach (var item in movement.Items)
            {
                if (item.Documents == null) continue;

                var itemDecisionResult = movementDecisionResult.AddDecision(item);
                foreach (var document in item.Documents)
                {
                    Debug.Assert(document.DocumentReference != null);
                    var notification = decisionContext.Notifications.Find(x =>
                        x._MatchReference == MatchIdentifier.FromCds(document.DocumentReference).Identifier);

                    if (notification is null)
                    {
                        if (decisionContext.GenerateNoMatch)
                        {
                            itemDecisionResult.AddDecision(document, DecisionCode.X00);
                        }
                    }
                    else
                    {
                        var decision = GetDecision(notification);
                        Console.WriteLine(decision);
                        ////itemDecisionResult.AddDecision(document, decision.DecisionCode);
                    }
                }
            }
        }

        return Task.FromResult(decisionResult);
        
    }

    private static DecisionFinderResult GetDecision(ImportNotification notification)
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
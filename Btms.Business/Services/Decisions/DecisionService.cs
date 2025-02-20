using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Business.Services.Decisions.Finders;
using Btms.Business.Services.Matching;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Btms.Types.Alvs.Mapping;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Services.Decisions;

public class DecisionService(
    ILogger<DecisionService> logger,
    IEnumerable<IDecisionFinder> decisionFinders,
    MovementBuilderFactory movementBuilderFactory,
    IMongoDbContext dbContext) : IDecisionService
{
    public async Task<DecisionResult> Process(DecisionContext decisionContext, CancellationToken cancellationToken)
    {
        var decisionResult = await DeriveDecision(decisionContext);

        var messages = await DecisionMessageBuilder.Build(decisionContext, decisionResult);

        foreach (var message in messages)
        {
            decisionResult.AddDecisionMessage(message);
        }

        var notificationContext = decisionContext.Notifications
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
            .ToList();

        foreach (var decisionResultDecisionsMessage in decisionResult.DecisionsMessages)
        {
            var internalDecision = DecisionMapper.Map(decisionResultDecisionsMessage);
            var m = decisionContext.Movements.First(m =>
                m.Id!.Equals(decisionResultDecisionsMessage.Header?.EntryReference));
            var existingMovementBuilder = movementBuilderFactory
                .From(m)
                .MergeDecision(decisionContext.MessageId, internalDecision, notificationContext);
            m = existingMovementBuilder.Build();
            await dbContext.Movements.Update(m, cancellationToken);
        }

        return decisionResult;
    }

    [SuppressMessage("SonarLint", "S1905",
        Justification =
            "The Cast<string>() on line 64 is required to force the resulting variable to string[]? rather than string?[]?")]
    private Task<DecisionResult> DeriveDecision(DecisionContext decisionContext)
    {
        var decisionsResult = new DecisionResult();
        foreach (var movement in decisionContext.Movements)
        {
            foreach (var item in movement.Items)
            {
                if (decisionContext.HasChecks(movement.Id!, item.ItemNumber!.Value))
                {
                    ////var movement = decisionContext.Movements.First(x => x.Id == noMatch.MovementId);
                    var checkCodes = movement.Items.First(x => x.ItemNumber == item.ItemNumber!.Value).Checks
                        ?.Select(x => x.CheckCode).Where(x => x != null).Cast<string>().ToArray();
                    HandleNoMatches(decisionContext, item, movement, checkCodes, decisionsResult);

                    HandleMatches(decisionContext, item, movement, checkCodes, decisionsResult);

                    HandleItemsWithInvalidReference(movement.Id!, item, decisionsResult);
                }
            }
        }

        return Task.FromResult(decisionsResult);
    }


    private void HandleMatches(DecisionContext decisionContext, Items item, Movement movement, string[]? checkCodes,
        DecisionResult decisionsResult)
    {
        Debug.Assert(item.ItemNumber != null, "item.ItemNumber != null");
        int itemNumber = item.ItemNumber.Value;
        var matches = decisionContext.MatchingResult.Matches.Where(x =>
            x.ItemNumber == itemNumber &&
            x.MovementId == movement.Id).ToList();

        foreach (var match in matches)
        {
            var notification = decisionContext.Notifications.First(x => x.Id == match.NotificationId);

            var decisionCodes = GetDecisions(notification, checkCodes);
            foreach (var decisionCode in decisionCodes)
            {
                decisionsResult.AddDecision(match.MovementId, match.ItemNumber, match.DocumentReference,
                    decisionCode.CheckCode, decisionCode.DecisionCode, decisionCode.DecisionReason);
            }
        }
    }

    private static void HandleNoMatches(DecisionContext decisionContext, Items item, Movement movement,
        string[]? checkCodes, DecisionResult decisionsResult)
    {
        Debug.Assert(item.ItemNumber != null, "item.ItemNumber != null");
        int itemNumber = item.ItemNumber.Value;
        var noMatches = decisionContext.MatchingResult.NoMatches.Where(x =>
            x.ItemNumber == itemNumber &&
            x.MovementId == movement.Id).ToList();

        foreach (var noMatch in noMatches)
        {
            HandleNoMatch(checkCodes, decisionsResult, noMatch);
        }
    }

    private static void HandleNoMatch(string[]? checkCodes, DecisionResult decisionsResult, DocumentNoMatch noMatch)
    {
        if (checkCodes != null)
        {
            foreach (var checkCode in checkCodes)
            {
                string? reason = null;

                if (checkCode is "H220")
                {
                    reason =
                        "A Customs Declaration with a GMS product has been selected for HMI inspection. In IPAFFS create a CHEDPP and amend your licence to reference it. If a CHEDPP exists, amend your licence to reference it. Failure to do so will delay your Customs release";
                }

                decisionsResult.AddDecision(noMatch.MovementId, noMatch.ItemNumber,
                    noMatch.DocumentReference, checkCode, DecisionCode.X00, reason);
            }
        }
        else
        {
            decisionsResult.AddDecision(noMatch.MovementId, noMatch.ItemNumber,
                noMatch.DocumentReference, null, DecisionCode.X00);
        }
    }

    private static void HandleItemsWithInvalidReference(string movementId, Items item, DecisionResult decisionsResult)
    {
        Debug.Assert(item.ItemNumber != null, "item.ItemNumber != null");
        int itemNumber = item.ItemNumber.Value;
        var decisions = decisionsResult.Decisions.Where(x =>
            x.ItemNumber == itemNumber &&
            x.MovementId == movementId).ToList();

        if (!decisions.Any())
        {
            foreach (var document in item.Documents!)
            {
                decisionsResult.AddDecision(movementId, itemNumber,
                    document.DocumentReference!, null, DecisionCode.E89);
            }
        }
    }

    private DecisionFinderResult[] GetDecisions(ImportNotification notification, string[]? checkCodes)
    {
        var results = new List<DecisionFinderResult>();
        if (checkCodes == null)
        {
            results.AddRange(GetDecisionsForCheckCode(notification, null));
        }
        else
        {
            foreach (var checkCode in checkCodes)
            {
                results.AddRange(GetDecisionsForCheckCode(notification, checkCode));
            }
        }

        var item = 1;
        foreach (var result in results)
            logger.LogInformation(
                "Decision finder result {ItemNum} of {NumItems} for Notification {Id} Decision {Decision} - ConsignmentAcceptable {ConsignmentAcceptable}: DecisionEnum {DecisionEnum}: NotAcceptableAction {NotAcceptableAction}",
                item++, results.Count, notification.Id, result.DecisionCode.ToString(),
                notification.PartTwo?.Decision?.ConsignmentAcceptable,
                notification.PartTwo?.Decision?.DecisionEnum.ToString(),
                notification.PartTwo?.Decision?.NotAcceptableAction?.ToString());

        return results.ToArray();
    }

    private IEnumerable<DecisionFinderResult> GetDecisionsForCheckCode(ImportNotification notification,
        string? checkCode)
    {
        var finders = decisionFinders.Where(x => x.CanFindDecision(notification, checkCode)).ToArray();

        if (!finders.Any())
        {
            logger.LogWarning("No Decision Finder count for ImportNotification {Id} and Check code {CheckCode}",
                notification.Id, checkCode);
            yield return new DecisionFinderResult(DecisionCode.E90, checkCode);
        }

        foreach (var finder in finders)
        {
            yield return finder.FindDecision(notification, checkCode);
        }
    }
}
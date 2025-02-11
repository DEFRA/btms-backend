using System.Diagnostics.CodeAnalysis;
using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Btms.Types.Alvs.Mapping;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Services.Decisions;

public class DecisionService(ILogger<DecisionService> logger, IEnumerable<IDecisionFinder> decisionFinders,
    MovementBuilderFactory movementBuilderFactory, IMongoDbContext dbContext) : IDecisionService
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
            var m = decisionContext.Movements.First(m => m.Id!.Equals(decisionResultDecisionsMessage.Header?.EntryReference));
            var existingMovementBuilder = movementBuilderFactory
                .From(m)
                .MergeDecision(decisionContext.MessageId, internalDecision, notificationContext);
            m = existingMovementBuilder.Build();
            await dbContext.Movements.Update(m, cancellationToken);
        }

        return decisionResult;
    }

    [SuppressMessage("SonarLint", "S1905", Justification = "The Cast<string>() on line 64 is required to force the resulting variable to string[]? rather than string?[]?")]
    private Task<DecisionResult> DeriveDecision(DecisionContext decisionContext)
    {
        var decisionsResult = new DecisionResult();
        if (decisionContext.GenerateNoMatch)
        {
            foreach (var noMatch in decisionContext.MatchingResult.NoMatches)
            {
                if (decisionContext.HasChecks(noMatch.MovementId, noMatch.ItemNumber))
                {
                    decisionsResult.AddDecision(noMatch.MovementId, noMatch.ItemNumber, noMatch.DocumentReference, null, DecisionCode.X00);
                }
            }
        }

        foreach (var match in decisionContext.MatchingResult.Matches)
        {
            if (!decisionContext.HasChecks(match.MovementId, match.ItemNumber)) continue;

            var notification = decisionContext.Notifications.First(x => x.Id == match.NotificationId);
            var movement = decisionContext.Movements.First(x => x.Id == match.MovementId);
            var checkCodes = movement.Items.First(x => x.ItemNumber == match.ItemNumber).Checks?.Select(x => x.CheckCode).Where(x => x != null).Cast<string>().ToArray();
            var decisionCodes = GetDecisions(notification, checkCodes);
            foreach (var decisionCode in decisionCodes)
                decisionsResult.AddDecision(match.MovementId, match.ItemNumber, match.DocumentReference, decisionCode.CheckCode, decisionCode.DecisionCode, decisionCode.DecisionReason);
        }

        return Task.FromResult(decisionsResult);
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
            logger.LogInformation("Decision finder result {ItemNum} of {NumItems} for Notification {Id} Decision {Decision} - ConsignmentAcceptable {ConsignmentAcceptable}: DecisionEnum {DecisionEnum}: NotAcceptableAction {NotAcceptableAction}",
                item++, results.Count, notification.Id, result.DecisionCode.ToString(), notification.PartTwo?.Decision?.ConsignmentAcceptable, notification.PartTwo?.Decision?.DecisionEnum.ToString(), notification.PartTwo?.Decision?.NotAcceptableAction?.ToString());

        return results.ToArray();
    }

    private IEnumerable<DecisionFinderResult> GetDecisionsForCheckCode(ImportNotification notification, string? checkCode)
    {
        var finders = decisionFinders.Where(x => x.CanFindDecision(notification, checkCode)).ToArray();

        foreach (var finder in finders)
        {
            yield return finder.FindDecision(notification, checkCode);
        }
    }
}
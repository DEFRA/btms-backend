using Btms.Business.Extensions;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;

namespace Btms.Business.Services.Decisions;

public static class DecisionMessageBuilder
{
    public static Task<List<CdsDecision>> Build(DecisionContext decisionContext, DecisionResult decisionResult)
    {
        var list = new List<CdsDecision>();

        var decisionsByMovement = decisionResult.Decisions.GroupBy(x => x.MovementId);

        foreach (var movementDecisions in decisionsByMovement)
        {
            var movement = decisionContext.Movements.First(x => x.Id == movementDecisions.Key);
            var messageNumber = movement is { Decisions: null } ? 1 : movement.Decisions.Count + 1;
            var decisionMessage = new CdsDecision
            {
                ServiceHeader = BuildServiceHeader(),
                Header = BuildHeader(movement, messageNumber),
                Items = BuildItems(movement, movementDecisions).ToArray()
            };
            list.Add(decisionMessage);
        }

        return Task.FromResult(list);
    }

    private static ServiceHeader BuildServiceHeader()
    {
        return new ServiceHeader
        {
            SourceSystem = "BTMS",
            ServiceCalled = DateTime.UtcNow,
            DestinationSystem = "CDS",
            CorrelationId = Guid.NewGuid().ToString()
        };
    }

    private static DecisionHeader BuildHeader(Movement movement, int messageNumber)
    {
        return new DecisionHeader
        {
            EntryReference = movement.EntryReference,
            EntryVersionNumber = movement.EntryVersionNumber,
            DecisionNumber = messageNumber
        };
    }

    private static IEnumerable<DecisionItems> BuildItems(Movement movement, IGrouping<string, DocumentDecisionResult> movementDecisions)
    {
        var decisionsByItem = movementDecisions.GroupBy(x => x.ItemNumber);
        foreach (var itemDecisions in decisionsByItem)
        {
            var item = movement.Items.First(x => x.ItemNumber == itemDecisions.Key);
            yield return new DecisionItems
            {
                ItemNumber = itemDecisions.Key,
                Checks = BuildChecks(item, itemDecisions).ToArray()
            };
        }
    }

    private static IEnumerable<DecisionCheck> BuildChecks(Model.Cds.Items item, IGrouping<int, DocumentDecisionResult> itemDecisions)
    {
        if (item.Checks == null) yield break;

        foreach (var checkCode in item.Checks.Select(x => x.CheckCode!))
        {
            var maxDecisionResult = itemDecisions.Where(x => x.CheckCode == null || x.CheckCode == checkCode).OrderByDescending(x => x.DecisionCode).FirstOrDefault();
            if (maxDecisionResult is not null)
            {
                yield return new DecisionCheck
                {
                    CheckCode = checkCode,
                    DecisionCode = maxDecisionResult.DecisionCode.ToString(),
                    DecisionReasons = BuildDecisionReasons(item, maxDecisionResult!),
                    DecisionInternalFurtherDetail =
                        maxDecisionResult.InternalDecisionCode.HasValue ?
                            [maxDecisionResult.InternalDecisionCode.Value.ToString()] :
                            null
                };
            }
        }
    }

    private static string[] BuildDecisionReasons(Model.Cds.Items item, DocumentDecisionResult maxDecisionResult)
    {
        var reasons = new List<string>();

        if (maxDecisionResult.DecisionReason != null)
        {
            reasons.Add(maxDecisionResult.DecisionReason);
        }


        if (maxDecisionResult.DecisionCode == DecisionCode.X00)
        {
            var chedType = MapToChedType(item.Documents?[0].DocumentCode!);
            var chedNumbers = string.Join(',', item.Documents!.Select(x => x.DocumentReference));

            if (!reasons.Any())
            {
                reasons.Add(
                    $"A Customs Declaration has been submitted however no matching {chedType}(s) have been submitted to Port Health (for {chedType} number(s) {chedNumbers}). Please correct the {chedType} number(s) entered on your customs declaration.");
            }
        }

        return reasons.ToArray();
    }

    private static string MapToChedType(string documentCode)
    {
        var ct = documentCode.GetChedType();

        if (!ct.HasValue())
        {
            throw new ArgumentOutOfRangeException(nameof(documentCode), documentCode, null);
        }

        return ct.ToString()!;
    }
}
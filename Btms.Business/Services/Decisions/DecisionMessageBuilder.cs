using Btms.Business.Extensions;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Types.Alvs;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Business.Services.Decisions;

public static class DecisionMessageBuilder
{
    public static Task<List<Decision>> Build(DecisionContext decisionContext, DecisionResult decisionResult)
    {
        var list = new List<Decision>();

        var decisionsByMovement = decisionResult.Decisions.GroupBy(x => x.MovementId);

        foreach (var movementDecisions in decisionsByMovement)
        {
            var movement = decisionContext.Movements.First(x => x.Id == movementDecisions.Key);
            var messageNumber = movement is { Decisions: null } ? 1 : movement.Decisions.Count + 1;
            var decisionMessage = new Decision
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
            ServiceCallTimestamp = DateTime.UtcNow,
            DestinationSystem = "CDS",
            CorrelationId = Guid.NewGuid().ToString()
        };
    }

    private static Header BuildHeader(Movement movement, int messageNumber)
    {
        return new Header
        {
            EntryReference = movement.EntryReference,
            EntryVersionNumber = movement.EntryVersionNumber,
            DecisionNumber = messageNumber
        };
    }
    
    private static IEnumerable<Items> BuildItems(Movement movement, IGrouping<string, DocumentDecisionResult> movementDecisions)
    {
        var decisionsByItem = movementDecisions.GroupBy(x => x.ItemNumber);
        foreach (var itemDecisions in decisionsByItem)
        {
            var item = movement.Items.First(x => x.ItemNumber == itemDecisions.Key);
            yield return new Items
            {
                ItemNumber = itemDecisions.Key,
                Checks = BuildChecks(item, itemDecisions).ToArray()
            };
        }
    }

    private static IEnumerable<Check> BuildChecks(Model.Cds.Items item, IGrouping<int, DocumentDecisionResult> itemDecisions)
    {
        if (item.Checks == null) yield break;
        
        foreach (var checkCode in item.Checks.Select(x => x.CheckCode))
        {
            var maxDecisionResult = itemDecisions.Where(x => x.CheckCode == null || x.CheckCode == checkCode).OrderByDescending(x => x.DecisionCode).First();
            yield return new Check
            {
                CheckCode = checkCode,
                DecisionCode = maxDecisionResult.DecisionCode.ToString(),
                DecisionReasons = BuildDecisionReasons(item, maxDecisionResult)
            };
        }
    }

    private static string[] BuildDecisionReasons(Model.Cds.Items item, DocumentDecisionResult maxDecisionResult)
    {
        var reasons = new List<string>();
        if (maxDecisionResult.DecisionCode == DecisionCode.X00)
        {
            var chedType = MapToChedType(item.Documents?[0].DocumentCode!);
            var chedNumbers = string.Join(',', item.Documents!.Select(x => x.DocumentReference));
            reasons.Add($"A Customs Declaration has been submitted however no matching {chedType}(s) have been submitted to Port Health (for {chedType} number(s) {chedNumbers}). Please correct the {chedType} number(s) entered on your customs declaration.");
        }

        if (maxDecisionResult.DecisionReason != null)
        {
            reasons.Add(maxDecisionResult.DecisionReason);
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
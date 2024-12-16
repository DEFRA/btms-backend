using Btms.Model;
using Btms.Model.Ipaffs;
using Btms.Types.Alvs;

namespace Btms.Business.Services.Decisions;

public static class DecisionMessageBuilder
{
    public static Task<List<AlvsClearanceRequest>> Build(DecisionContext decisionContext, DecisionResult decisionResult)
    {
        var list = new List<AlvsClearanceRequest>();

        var movements = decisionResult.Decisions.GroupBy(x => x.MovementId).ToList();

        foreach (var movementGroup in movements)
        {
            var movement = decisionContext.Movements.First(x => x.Id == movementGroup.Key);
            var messageNumber = movement is { Decisions: null } ? 1 : movement.Decisions.Count + 1;
            var decisionMessage = new AlvsClearanceRequest()
            {
                ServiceHeader = BuildServiceHeader(),
                Header = BuildHeader(movement, messageNumber),
                Items = BuildItems(movement, movementGroup).ToArray()
            };
            list.Add(decisionMessage);
        }

        return Task.FromResult(list);
    }

    private static ServiceHeader BuildServiceHeader()
    {
        return new ServiceHeader()
        {
            SourceSystem = "BTMS",
            ServiceCallTimestamp = DateTime.UtcNow,
            DestinationSystem = "CDS",
            CorrelationId = Guid.NewGuid().ToString()
        };
    }

    private static Header BuildHeader(Movement movement, int messageNumber)
    {
        return new Header()
        {
            EntryReference = movement.EntryReference,
            EntryVersionNumber = movement.EntryVersionNumber,
            DecisionNumber = messageNumber
        };
    }


   

    private static IEnumerable<Items> BuildItems(Movement movement, IGrouping<string, DocumentDecisionResult> movementGroup)
    {
        var itemGroups = movementGroup.GroupBy(x => x.ItemNumber);
        foreach (var itemGroup in itemGroups)
        {
            var item = movement.Items.First(x => x.ItemNumber == itemGroup.Key);
            yield return new Items()
            {
                ItemNumber = itemGroup.Key,
                Checks = BuildChecks(item, itemGroup).ToArray()
            };
        }
    }

    private static IEnumerable<Check> BuildChecks(Model.Alvs.Items item, IGrouping<int, DocumentDecisionResult> itemsGroup)
    {
        if (item.Checks != null)
        {
            foreach (var itemCheck in item.Checks)
            {
                var decisionCode = itemsGroup.Max(x => x.DecisionCode);
                yield return new Check()
                {
                    CheckCode = itemCheck.CheckCode,
                    DecisionCode = itemsGroup.Max(x => x.DecisionCode).ToString(),
                    DecisionReasons = BuildDecisionReasons(item, decisionCode)
                };
            }
        }
    }

    public static string[] BuildDecisionReasons(Model.Alvs.Items item, DecisionCode decisionCode)
    {
        switch (decisionCode)
        {
            case DecisionCode.X00:
                var chedType = MapToChedType(item.Documents?[0].DocumentCode!);
                var chedNumbers = string.Join(',', item.Documents!.Select(x => x.DocumentReference));
                return
                [
                    $"A Customs Declaration has been submitted however no matching {chedType}(s) have been submitted to Port Health (for {chedType} number(s) {chedNumbers}). Please correct the {chedType} number(s) entered on your customs declaration."
                ];
        }

        return [];
    }

    private static string MapToChedType(string documentCode)
    {
        return documentCode switch
        {
            "N002" or "N851" or "9115" => ImportNotificationTypeEnum.Chedpp.ToString(),
            "N852" or "C678" => ImportNotificationTypeEnum.Ced.ToString(),
            "C640" => ImportNotificationTypeEnum.Cveda.ToString(),
            "C641" or "C673" or "N853" => ImportNotificationTypeEnum.Cvedp.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(documentCode), documentCode, null)
        };


    }
}
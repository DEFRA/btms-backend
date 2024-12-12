using Btms.Types.Alvs;

namespace Btms.Business.Services.Decisions;

public interface IMessageNumberProvider
{
    Task<int> Next(string entryReference);
}

public class MemoryMessageNumberProvider : IMessageNumberProvider
{
    private readonly Dictionary<string, int> _sequenceNumbers = new Dictionary<string, int>();
    public Task<int> Next(string entryReference)
    {
        if (_sequenceNumbers.TryGetValue(entryReference, out var value))
        {
            _sequenceNumbers[entryReference] = value + 1;
        }
        else
        {
            _sequenceNumbers[entryReference] = 1;
        }

        return Task.FromResult(_sequenceNumbers[entryReference]);
    }
}

public class DecisionMessageBuilder(IMessageNumberProvider messageNumberProvider) : IDecisionMessageBuilder
{
    public async Task<List<AlvsClearanceRequest>> Build(DecisionResult decisionResult)
    {
        var list = new List<AlvsClearanceRequest>();
        foreach (var decision in decisionResult.MovementDecisions)
        {
            var messageNumber = await messageNumberProvider.Next(decision.EntryReference);
            var decisionMessage = new AlvsClearanceRequest()
            {
                ServiceHeader = BuildServiceHeader(),
                Header = BuildHeader(decision, messageNumber),
                Items = BuildItems(decision).ToArray()
            };

            list.Add(decisionMessage);
        }

        return list;
    }

    private ServiceHeader BuildServiceHeader()
    {
        return new ServiceHeader()
        {
            SourceSystem = "BTMS",
            ServiceCallTimestamp = DateTime.UtcNow,
            DestinationSystem = "CDS",
            CorrelationId = Guid.NewGuid().ToString()
        };
    }

    private Header BuildHeader(MovementDecisionResult decision, int messageNumber)
    {
        return new Header()
        {
            EntryReference = decision.EntryReference, EntryVersionNumber = decision.EntryVersion,
            DecisionNumber = messageNumber
        };
    }


    private IEnumerable<Check> BuildChecks(ItemDecisionResult itemDecision)
    {
        if (itemDecision.Item.Checks != null)
        {
            foreach (var itemCheck in itemDecision.Item.Checks)
            {
                yield return new Check()
                {
                    CheckCode = itemCheck.CheckCode,
                    DecisionCode = itemDecision.GetDecisionCode().ToString(),
                    DecisionReasons = itemDecision.GetDecisionReasons()
                };
            }
        }
    }

    private IEnumerable<Items> BuildItems(MovementDecisionResult movementDecision)
    {
        foreach (var itemDecision in movementDecision.ItemDecisions)
        {
            yield return new Items()
            {
                ItemNumber = itemDecision.Item.ItemNumber,
                Checks = BuildChecks(itemDecision).ToArray()
            };
        }
    }
}
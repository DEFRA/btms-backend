namespace Btms.Business.Services.Decisions;

public record DecisionResult
{
    private readonly List<DocumentDecisionResult> _results = [];
    public DocumentDecisionResult AddDecision(string movementId, int itemNumber, string documentReference, DecisionCode decisionCode)
    {
        var item = new DocumentDecisionResult(movementId, itemNumber, documentReference, decisionCode);
        _results.Add(item);
        return item;
    }

    public IReadOnlyList<DocumentDecisionResult> Decisions => _results.AsReadOnly();
}

public record DocumentDecisionResult(string MovementId, int ItemNumber, string DocumentReference, DecisionCode DecisionCode);

public record DecisionFinderResult(DecisionCode DecisionCode, string? DecisionReason = null);

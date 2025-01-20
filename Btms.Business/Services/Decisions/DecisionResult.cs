namespace Btms.Business.Services.Decisions;

public record DecisionResult
{
    private readonly List<DocumentDecisionResult> _results = [];
    
    public void AddDecision(string movementId, int itemNumber, string documentReference, DecisionCode decisionCode, string? decisionReason = null)
    {
        _results.Add(new DocumentDecisionResult(movementId, itemNumber, documentReference, decisionCode, decisionReason));
    }

    public IReadOnlyList<DocumentDecisionResult> Decisions => _results.AsReadOnly();
}

public record DocumentDecisionResult(string MovementId, int ItemNumber, string DocumentReference, DecisionCode DecisionCode, string? DecisionReason);

public record DecisionFinderResult(DecisionCode DecisionCode, string? DecisionReason = null);

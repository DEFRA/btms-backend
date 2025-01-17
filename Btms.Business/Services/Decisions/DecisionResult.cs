namespace Btms.Business.Services.Decisions;

public record DecisionResult
{
    private readonly List<DocumentDecisionResult> _results = [];
    
    public void AddDecision(string movementId, int itemNumber, string documentReference, DecisionCode decisionCode, DecisionType decisionType, string? decisionReason = null)
    {
        _results.Add(new DocumentDecisionResult(movementId, itemNumber, documentReference, decisionCode, decisionType, decisionReason));
    }

    public IReadOnlyList<DocumentDecisionResult> Decisions => _results.AsReadOnly();
}

public record DocumentDecisionResult(string MovementId, int ItemNumber, string DocumentReference, DecisionCode DecisionCode, DecisionType DecisionType, string? DecisionReason);

public record DecisionFinderResult(DecisionCode DecisionCode, DecisionType DecisionType, string? DecisionReason = null);

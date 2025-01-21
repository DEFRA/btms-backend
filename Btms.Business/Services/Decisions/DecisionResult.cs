namespace Btms.Business.Services.Decisions;

public record DecisionResult
{
    private readonly List<DocumentDecisionResult> _results = [];
    
    public void AddDecision(string movementId, int itemNumber, string documentReference, string? checkCode, DecisionCode decisionCode, string? decisionReason = null)
    {
        _results.Add(new DocumentDecisionResult(movementId, itemNumber, documentReference, checkCode, decisionCode, decisionReason));
    }

    public IReadOnlyList<DocumentDecisionResult> Decisions => _results.AsReadOnly();
}

public record DocumentDecisionResult(string MovementId, int ItemNumber, string DocumentReference, string? CheckCode, DecisionCode DecisionCode, string? DecisionReason);

public record DecisionFinderResult(DecisionCode DecisionCode, string? CheckCode, string? DecisionReason = null);

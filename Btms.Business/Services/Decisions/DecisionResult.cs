using Btms.Model.Cds;
using Btms.Types.Alvs;

namespace Btms.Business.Services.Decisions;

public record DecisionResult
{
    private readonly List<DocumentDecisionResult> _results = [];
    private readonly List<Decision> _decisionMessages = [];

    public void AddDecision(string movementId, int itemNumber, string documentReference, string? checkCode, DecisionCode decisionCode, string? decisionReason = null)
    {
        _results.Add(new DocumentDecisionResult(movementId, itemNumber, documentReference, checkCode, decisionCode, decisionReason));
    }

    public void AddDecisionMessage(Decision message)
    {
        _decisionMessages.Add(message);
    }

    public IReadOnlyList<DocumentDecisionResult> Decisions => _results.AsReadOnly();

    public IReadOnlyList<Decision> DecisionsMessages => _decisionMessages.AsReadOnly();
}

public record DocumentDecisionResult(string MovementId, int ItemNumber, string DocumentReference, string? CheckCode, DecisionCode DecisionCode, string? DecisionReason);

public record DecisionFinderResult(DecisionCode DecisionCode, string? CheckCode, string? DecisionReason = null);
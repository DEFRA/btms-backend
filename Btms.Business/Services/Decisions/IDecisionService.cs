namespace Btms.Business.Services.Decisions;

public interface IDecisionService
{
    public Task<DecisionResult> DeriveDecision(DecisionContext decisionContext);
}
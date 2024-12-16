namespace Btms.Business.Services.Decisions;

public interface IDecisionService
{
    public Task<DecisionResult> Process(DecisionContext decisionContext, CancellationToken cancellationToken);
}
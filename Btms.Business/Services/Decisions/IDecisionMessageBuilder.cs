using Btms.Types.Alvs;

namespace Btms.Business.Services.Decisions;

public interface IDecisionMessageBuilder
{
    Task<List<AlvsClearanceRequest>> Build(DecisionResult decisionResult);
}
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public interface IDecisionFinder
{
    DecisionResult FindDecision(ImportNotification notification);
}
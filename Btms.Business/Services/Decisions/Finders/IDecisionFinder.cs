using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public interface IDecisionFinder
{
    DecisionFinderResult FindDecision(ImportNotification notification);
}
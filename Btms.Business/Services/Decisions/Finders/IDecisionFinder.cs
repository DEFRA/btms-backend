using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public interface IDecisionFinder
{
    bool CanFindDecision(ImportNotification notification);
    DecisionFinderResult FindDecision(ImportNotification notification);
}
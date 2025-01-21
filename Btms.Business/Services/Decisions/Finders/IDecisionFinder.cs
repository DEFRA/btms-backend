using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions.Finders;

public interface IDecisionFinder
{
    bool CanFindDecision(ImportNotification notification, string? checkCode);
    DecisionFinderResult FindDecision(ImportNotification notification, string? checkCode);
}
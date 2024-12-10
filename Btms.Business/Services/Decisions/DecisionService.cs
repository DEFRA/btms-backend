using Btms.Business.Services.Decisions.Finders;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions;

public class DecisionService : IDecisionService
{
    public async Task<DecisionResult> DeriveDecision(DecisionContext decisionContext)
    {
        // Validate and prerequisite checks
        
        var decisions = decisionContext.Notifications.Select(x => (x.Id, GetDecision(x))).ToList();
        
        foreach (var movement in decisionContext.Movements)
        {
            // Generate list matched items -> decisions
            
            foreach (var item in movement.Items)
            {
                // check decisions list for match reference, if no match then drop out with "no-match"
                
            }
            
            // Return decision based on prioritisation from confluence
        }
        
        await Task.CompletedTask;
        return new DecisionResult(DecisionCode.N02);
    }

    private DecisionResult GetDecision(ImportNotification notification)
    {
        // get decision finder - fold IUU stuff into the decision finder for fish?
        IDecisionFinder finder;
        switch (notification.ImportNotificationType)
        {
            case ImportNotificationTypeEnum.Ced:
                finder = new ChedDDecisionFinder();
                break;
            
            case ImportNotificationTypeEnum.Cveda:
                finder = new  ChedADecisionFinder();
                break;

            case ImportNotificationTypeEnum.Cvedp:
                finder = new  ChedPDecisionFinder();
                break;

            case ImportNotificationTypeEnum.Chedpp:
                finder = new  ChedPPDecisionFinder();
                break;

            default: throw new InvalidOperationException("Invalid ImportNotificationType");
        }
        
        return finder.FindDecision(notification);
    }
}
using Btms.Model;

namespace Btms.Business.Extensions;

public static class MovementExtensions
{
    public static void AddLinkStatus(this Movement movement)
    {
        var linkStatus = "Not Linked";

        if (movement.Relationships.Notifications.Data.Count > 0)
        {
            linkStatus = "Linked";
        }
        else if (movement.AlvsDecisionStatus?.Context?.AlvsCheckStatus?.AnyMatch ?? false)
        {
            linkStatus = "Investigate";
        }
        
        movement.Status.LinkStatus = linkStatus;
    }
    
}
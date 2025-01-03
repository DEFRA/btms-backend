using Btms.Model;
using Btms.Model.Cds;

namespace Btms.Business.Extensions;

public static class MovementExtensions
{
    public static void AddLinkStatus(this Movement movement)
    {
        var linkStatus = MovementStatus.NotLinkedStatus;
        var linked = false;
        
        if (movement.Relationships.Notifications.Data.Count > 0)
        {
            linkStatus = MovementStatus.LinkedStatus;
            linked = true;
        }
        else if (movement.AlvsDecisionStatus?.Context?.AlvsCheckStatus?.AnyMatch ?? false)
        {
            linkStatus = MovementStatus.InvestigateStatus;
        }
        
        movement.BtmsStatus.LinkStatus = linkStatus;
        movement.BtmsStatus.Linked = linked;
    }
}
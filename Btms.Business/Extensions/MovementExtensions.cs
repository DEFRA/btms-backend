using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;

namespace Btms.Business.Extensions;

public static class MovementExtensions
{
    public static bool AreNumbersComplete<T>(this IEnumerable<T> source, Func<T, int?> getNumbers)
    {
        var numbers = source
            .Select(getNumbers)
            .Where(n => n.HasValue())
            .Order()
            .ToList();

        if (numbers.Distinct().Count() != numbers.Count())
        {
            //Contains duplicates
            return false;
        }
        else if (numbers.Count() != numbers.Last())
        {
            //Some missing
            // should be contiguous

            return false;
        }

        return true;
    }
    
    public static void AddLinkStatus(this Movement movement)
    {
        if (movement.BtmsStatus.LinkStatus == LinkStatusEnum.Error) return;
        
        var linkStatus = LinkStatusEnum.NotLinked;
        var linked = false;
        
        if (movement.Relationships.Notifications.Data.Count > 0)
        {
            linkStatus = LinkStatusEnum.Linked;
            linked = true;
        }
        
        movement.BtmsStatus.LinkStatus = linkStatus;
        movement.BtmsStatus.Linked = linked;
    }
}
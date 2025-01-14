using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Analytics.Extensions;

public static class MovementExtensions
{   
    public static IQueryable<Movement> WhereFilteredByCreatedDateAndParams(this IQueryable<Movement> source, DateTime from, DateTime to,
        ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        return source
            .Where(m => (m.CreatedSource >= from && m.CreatedSource < to)
                        && (country == null || m.DispatchCountryCode == country)
                        && (chedTypes == null || !chedTypes!.Any() ||
                            !m.BtmsStatus.ChedTypes!.Any() ||
                            m.BtmsStatus.ChedTypes!.Any(c => chedTypes!.Contains(c))));

    }

    public class MovementWithLinkStatus
    {
        public required Movement Movement;
        public required DateTime CreatedSource;
        public required LinkStatusEnum Status;
    }
    
    public static IQueryable<MovementWithLinkStatus> SelectLinkStatus(this IQueryable<Movement> source)
    {
        var m = source
            .Select(m => new MovementWithLinkStatus() {
                Movement = m,
                CreatedSource = m.CreatedSource!.Value,
                Status = m.BtmsStatus.LinkStatus
            });

        return m;
    } 
}
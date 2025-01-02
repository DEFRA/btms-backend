using Btms.Model;
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
                            !m.AlvsDecisionStatus!.Context!.ChedTypes!.Any() ||
                            m.AlvsDecisionStatus!.Context!.ChedTypes!.Any(c => chedTypes!.Contains(c))));

    } 
}
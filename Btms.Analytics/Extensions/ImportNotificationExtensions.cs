using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Analytics.Extensions;

public static class ImportNotificationExtensions
{   
    public static IQueryable<ImportNotification> WhereFilteredByCreatedDateAndParams(this IQueryable<ImportNotification> source, DateTime from, DateTime to,
        ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        return source
            .Where(n => (n.CreatedSource >= from && n.CreatedSource < to)
                && (country == null || n.CommoditiesSummary!.CountryOfOrigin! == country)
                && (
                    chedTypes == null ||
                    chedTypes!.Length == 0 || n.ImportNotificationType == null ||
                    chedTypes!.Contains(n.ImportNotificationType!.Value)
                )
            );
    }
}
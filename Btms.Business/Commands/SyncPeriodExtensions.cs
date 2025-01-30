using Btms.Common.Extensions;
using Microsoft.VisualBasic;
using DateTime = System.DateTime;

namespace Btms.Business.Commands;

public static class SyncPeriodExtensions
{
    public static string[] GetPeriodPaths(this SyncPeriod period)
    {
        if (period == SyncPeriod.LastMonth)
        {
            return [DateTime.Today.AddMonths(-1).ToString("/yyyy/MM/")];
        }
        else if (period == SyncPeriod.ThisMonth)
        {
            return [DateTime.Today.ToString("/yyyy/MM/")];
        }
        else if (period == SyncPeriod.Today)
        {
            return [DateTime.Today.ToString("/yyyy/MM/dd/")];
        }
        else if (period == SyncPeriod.Yesterday)
        {
            return [DateTime.Today.AddDays(-1).ToString("/yyyy/MM/dd/")];
        }
        // else if (period == SyncPeriod.From202411)
        // {
        //     return DateTime.Today
        //         .MonthsSince(new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc))
        //         .Select(p => $"/{p.Year}/{p.Month:00}/")
        //         .ToArray();
        // }
        else if (period == SyncPeriod.All)
        {
            return ["/"];
        }
        else
        {
            throw new ArgumentException($"SyncPeriod {period} has not been mapped to paths");
        }
    }
}
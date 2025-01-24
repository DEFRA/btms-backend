using Microsoft.VisualBasic;

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
        else if (period == SyncPeriod.From202411)
        {
            // TOOO : Make better!
            // var startDate = new DateTime(2024, 11, 1);
            // var months = 
            // var months = DateTime.Today.Subtract(startDate);
            // return [DateTime.Today.ToString("/yyyy/MM/")];

            return ["/2024/11/", "/2024/12/", "/2025/01/"];
        }
        else if (period == SyncPeriod.All)
        {
            return ["/"];
        }
        else
        {
            throw new ArgumentException($"Unexpected SyncPeriod {period}");
        }
    }
}
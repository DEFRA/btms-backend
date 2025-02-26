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
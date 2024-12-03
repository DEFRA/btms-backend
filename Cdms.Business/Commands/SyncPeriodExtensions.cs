namespace Cdms.Business.Commands;

public static class SyncPeriodExtensions
{
    public static string GetPeriodPath(this SyncPeriod period)
    {
        if (period == SyncPeriod.LastMonth)
        {
            return DateTime.Today.AddMonths(-1).ToString("/yyyy/MM/");
        }
        else if (period == SyncPeriod.ThisMonth)
        {
            return DateTime.Today.ToString("/yyyy/MM/");
        }
        else if (period == SyncPeriod.Today)
        {
            return DateTime.Today.ToString("/yyyy/MM/dd/");
        }
        else if (period == SyncPeriod.All)
        {
            return "/";
        }
        else
        {
            throw new ArgumentException($"Unexpected SyncPeriod {period}");
        }
    }
}
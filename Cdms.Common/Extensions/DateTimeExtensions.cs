namespace Cdms.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime TrimMicroseconds(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
    }

    public static DateTime? TrimMicroseconds(this DateTime? dt)
    {
        return dt?.TrimMicroseconds();
    }
    public static DateTime TrimMinutes(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0, dt.Kind);
    }

    public static DateTime NextHour(this DateTime dt)
    {
        return dt.AddHours(-1).TrimMinutes();
    }
    
    public static DateTime CurrentHour(this DateTime dt)
    {
        return dt.TrimMinutes();
    }
    
    public static DateTime Yesterday(this DateTime dt)
    {
        return dt.AddDays(-1);
    }
    
    public static DateTime Tomorrow(this DateTime dt)
    {
        return dt.AddDays(1);
    }
    public static DateTime WeekAgo(this DateTime dt)
    {
        return dt.AddDays(-7);
    }
    public static DateTime WeekLater(this DateTime dt)
    {
        return dt.AddDays(7);
    }
    
    public static DateTime MonthAgo(this DateTime dt)
    {
        return dt.AddMonths(-1);
    }
    
    public static int DaysSinceMonthAgo(this DateTime dt)
    {
        return Convert.ToInt32((dt - dt.AddMonths(-1)).TotalDays) ;
    }
    
    public static int DaysUntilMonthLater(this DateTime dt)
    {
        return Convert.ToInt32((dt.AddMonths(1) - dt).TotalDays) ;
    }
    
    public static DateTime MonthLater(this DateTime dt)
    {
        return dt.AddMonths(1);
    }
    
    private static int CreateRandomInt(int min, int max)
    {
        return Random.Shared.Next(min, max);
    }
    
    public static DateTime RandomTime(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, CreateRandomInt(0,23), CreateRandomInt(0, 60), CreateRandomInt(0, 60), dt.Kind);
    }
    
    public static DateOnly ToDate(this DateTime val)
    {
        return DateOnly.FromDateTime(val);
    }
    
    public static DateOnly ToDate(this DateTime? val)
    {
        return val?.ToDate() ?? DateOnly.MinValue;
    }
    
    public static TimeOnly ToTime(this DateTime val)
    {
        return TimeOnly.FromDateTime(val);
    }
}
using System.Diagnostics;
using System.Text.Json;

namespace Cdms.Common.Extensions;

public static class GeneralExtensions
{
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj);
    }
    
    public static bool HasValue<T>(this T? val)
    {
        return !object.Equals(val, default(T));
    }
    
    public static void AssertHasValue<T>(this T? val, string message = "Missing value")
    {
        Debug.Assert(val.HasValue(),  message);
    }
    
    public static DateOnly ToDate(this DateTime val)
    {
        return DateOnly.FromDateTime(val);
    }
    
    public static TimeOnly ToTime(this DateTime val)
    {
        return TimeOnly.FromDateTime(val);
    }
}
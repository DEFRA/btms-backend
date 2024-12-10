using System.Diagnostics;
using System.Text.Json;

namespace Btms.Common.Extensions;

public static class GeneralExtensions
{
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj);
    }
    
    public static bool HasValue<T>(this T? val)
    {
        return !Equals(val, default(T));
    }
    
    public static void AssertHasValue<T>(this T? val, string message = "Missing value")
    {
        Debug.Assert(val.HasValue(),  message);
    }
}
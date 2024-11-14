using System.Diagnostics;
using System.Text.Json;

namespace Cdms.Common.Extensions;

public static class GeneralExtensions
{
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj);
    }
    
    public static bool HasValue(this string? val)
    {
        return val != null;
    }
    
    public static void AssertHasValue(this string? val)
    {
        Debug.Assert(val.HasValue());
    }
}
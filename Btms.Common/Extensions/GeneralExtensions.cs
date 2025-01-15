using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.Json;

namespace Btms.Common.Extensions;

public static class GeneralExtensions
{
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj);
    }
    
    [Pure] public static bool HasValue<T>([Pure, NotNullWhen(true)] this T? val)
    {
        if (!Equals(val, default(T)))
        {
            return true;
        }

        return false;
    }
    
    public static void AssertHasValue<T>(this T? val, string message = "Missing value")
    {
        Debug.Assert(val.HasValue(),  message);
    }
}
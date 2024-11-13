using System.Text.Json;

namespace Cdms.Common.Extensions;

public static class GeneralExtensions
{
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj);
    }
}
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Btms.Common.Extensions;

public static class StringExtensions
{
    public static JsonNode? ToJsonNode(this string value)
    {
        return JsonNode.Parse(value);
    }
    public static JsonObject? ToJsonObject(this string value)
    {
        return JsonNode.Parse(value) as JsonObject;
    }
    public static IDictionary<string, JsonNode>? ToJsonDictionary(this string value)
    {
        return JsonNode.Parse(value) as IDictionary<string, JsonNode>;
    }

    public static bool StartsWithLowercase(this string? s)
    {
        return !s.HasValue() || Char.IsLower(s![1]);
    }

    public static string FirstCharToLower(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToLower(), input.AsSpan(1))
        };
}
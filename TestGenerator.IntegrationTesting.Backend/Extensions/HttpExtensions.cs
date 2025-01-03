using System.Text.Json.Nodes;
using Btms.Common.Extensions;

namespace TestGenerator.IntegrationTesting.Backend.Extensions;

public static class HttpExtensions
{
    public static async Task<string> GetString(this HttpResponseMessage? response)
    {
        var s = await response!.Content.ReadAsStringAsync();
        return s;
    }
    
    public static async Task<IDictionary<string, JsonNode>> ToJsonDictionary(this HttpResponseMessage? response)
    {
        var s = await response!.GetString();
        var responseDictionary = s.ToJsonDictionary();
        ArgumentNullException.ThrowIfNull(responseDictionary);
        return responseDictionary;
    }
}
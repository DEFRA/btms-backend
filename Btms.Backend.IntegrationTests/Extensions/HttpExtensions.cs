using System.Text.Json.Nodes;
using Btms.Common.Extensions;

namespace Btms.Backend.IntegrationTests.Extensions;

public static class HttpExtensions
{
    public static async Task<IDictionary<string, JsonNode>> ToJsonDictionary(this HttpResponseMessage? response)
    {
        var responseDictionary = (await response!.Content.ReadAsStringAsync()).ToJsonDictionary();
        ArgumentNullException.ThrowIfNull(responseDictionary);
        return responseDictionary;
    }
}
using System.Text.Json;
using System.Text.Json.Nodes;
using Btms.Analytics;
using Btms.Common.Extensions;

namespace TestGenerator.IntegrationTesting.Backend.Extensions;

public static class HttpExtensions
{
    public static async Task<string> GetString(this HttpResponseMessage? response)
    {
        var s = await response!.Content.ReadAsStringAsync();
        return s;
    }

    public static async Task<T> As<T>(this HttpResponseMessage? response)
    {
        var s = await response!.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(s,
            new JsonSerializerOptions
            {
                // Converters = { new SingleOrManyDataConverterFactory() },
                PropertyNameCaseInsensitive = true
            }
        )!;
    }

    public static async Task<IDictionary<string, JsonNode>> ToJsonDictionary(this HttpResponseMessage? response)
    {
        var s = await response!.GetString();
        var responseDictionary = s.ToJsonDictionary();
        ArgumentNullException.ThrowIfNull(responseDictionary);
        return responseDictionary;
    }

    public static async Task<TDataset> AnalyticsChartAs<TDataset>(this HttpResponseMessage? response, string chart)
        // where TChart : IDimensionResult
        where TDataset : IDataset
    {
        var dict = await response.ToJsonDictionary();
        var s = dict[chart].ToJsonString();
        return JsonSerializer.Deserialize<TDataset>(s,
            new JsonSerializerOptions
            {
                // TODO : Refactor the JsonSerializerOptions used by the web project & re-use
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new DatasetResultTypeMappingConverter<TDataset>(),
                    // new DimensionResultTypeMappingConverter<TChart>() 
                }
            }
        )!;
    }
    public static async Task<TDataset> AnalyticsMultiSeriesChartAs<TDataset, TChart>(this HttpResponseMessage? response, string chart)
        where TChart : IDimensionResult
        where TDataset : IDataset
    {
        var dict = await response.ToJsonDictionary();
        var s = dict[chart].ToJsonString();
        return JsonSerializer.Deserialize<TDataset>(s,
            new JsonSerializerOptions
            {
                // TODO : Refactor the JsonSerializerOptions used by the web project & re-use
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new DatasetResultTypeMappingConverter<TDataset>(),
                    new DimensionResultTypeMappingConverter<TChart>()
                }
            }
        )!;
    }
}
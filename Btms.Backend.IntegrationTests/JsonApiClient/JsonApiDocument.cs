using System.Text.Json;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Objects;

namespace Btms.Backend.IntegrationTests.JsonApiClient;

public abstract class JsonApiDocument<T>
{
    protected JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        Converters = { new SingleOrManyDataConverterFactory() },
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [JsonPropertyName("links")] public TopLevelLinks? Links { get; set; }

    [JsonPropertyName("data")] public T Data { get; set; } = default!;

    [JsonPropertyName("errors")] public IList<ErrorObject>? Errors { get; set; }

    [JsonPropertyName("included")] public IList<ResourceObject>? Included { get; set; }

    [JsonPropertyName("meta")] public IDictionary<string, object?>? Meta { get; set; }
}
using System.Text.Json;
using JsonApiDotNetCore.Serialization.Objects;

namespace TestGenerator.IntegrationTesting.Backend.JsonApiClient;

public class ManyItemsJsonApiDocument : JsonApiDocument<List<ResourceObject>>
{
    public List<T> GetResourceObjects<T>()
    {
        return Data.Select(x =>
            JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(x.Attributes, JsonSerializerOptions),
                JsonSerializerOptions)).ToList()!;
    }
}
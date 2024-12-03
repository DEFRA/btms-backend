using System.Text.Json;
using JsonApiDotNetCore.Serialization.Objects;

namespace Btms.Backend.IntegrationTests.JsonApiClient;

public class ManyItemsJsonApiDocument : JsonApiDocument<List<ResourceObject>>
{
    public List<T> GetResourceObjects<T>()
    {
        return Data.Select(x =>
            JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(x.Attributes, jsonSerializerOptions),
                jsonSerializerOptions)).ToList()!;
    }
}
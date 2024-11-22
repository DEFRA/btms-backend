using System.Text.Json;
using JsonApiDotNetCore.Serialization.Objects;

namespace CdmsBackend.IntegrationTests.JsonApiClient;

public class SingleItemJsonApiDocument : JsonApiDocument<ResourceObject>
{
    public T GetResourceObject<T>()
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(this.Data.Attributes, jsonSerializerOptions),
            jsonSerializerOptions)!;
    }
}
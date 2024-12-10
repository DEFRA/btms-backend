using System.Text.Json;
using JsonApiDotNetCore.Serialization.Objects;

namespace Btms.Backend.IntegrationTests.JsonApiClient;

public class SingleItemJsonApiDocument : JsonApiDocument<ResourceObject>
{
    public T GetResourceObject<T>()
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(Data.Attributes, JsonSerializerOptions),
            JsonSerializerOptions)!;
    }
}
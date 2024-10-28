using System.Text.Json;

namespace Cdms.SensitiveData;

public interface ISensitiveDataSerializer
{
    public T Deserialize<T>(string json, Action<JsonSerializerOptions> optionsOverride = null);
}
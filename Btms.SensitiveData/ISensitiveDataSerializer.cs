using System.Text.Json;

namespace Btms.SensitiveData;

public interface ISensitiveDataSerializer
{
    public T Deserialize<T>(string json, Action<JsonSerializerOptions> optionsOverride = null!);

    string RedactRawJson(string json, Type type);
}
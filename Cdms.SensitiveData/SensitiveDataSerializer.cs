using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdms.SensitiveData;

public class SensitiveDataSerializer(ISensitiveDataOptions options) : ISensitiveDataSerializer
{
    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
    {
        TypeInfoResolver = new SensitiveDataTypeInfoResolver(options),
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public T Deserialize<T>(string json, Action<JsonSerializerOptions> optionsOverride = null)
    {
        JsonSerializerOptions newOptions = jsonOptions;
        if (optionsOverride is not null)
        {
            newOptions = new JsonSerializerOptions()
            {
                TypeInfoResolver = new SensitiveDataTypeInfoResolver(options),
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            optionsOverride(newOptions);
        }
        return JsonSerializer.Deserialize<T>(json, newOptions);
    }

    
}
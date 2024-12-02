using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cdms.SensitiveData;

public class SensitiveDataSerializer(IOptions<SensitiveDataOptions> options, ILogger<SensitiveDataSerializer> logger) : ISensitiveDataSerializer
{
    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
    {
        TypeInfoResolver = new SensitiveDataTypeInfoResolver(options.Value),
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public T Deserialize<T>(string json, Action<JsonSerializerOptions> optionsOverride = null!)
    {
        JsonSerializerOptions newOptions = jsonOptions;
        if (optionsOverride is not null)
        {
            newOptions = new JsonSerializerOptions()
            {
                TypeInfoResolver = new SensitiveDataTypeInfoResolver(options.Value),
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            optionsOverride(newOptions);
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, newOptions)!;
        }
#pragma warning disable S2139
        catch (Exception e)
#pragma warning restore S2139
        {
            logger.LogError(e, "Failed to Deserialize Json");
            throw;
        }
       
    }
}
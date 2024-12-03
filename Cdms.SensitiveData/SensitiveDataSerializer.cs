using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JsonFlatten;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

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

    public string RedactRawJson(string json, Type type)
    {
        var sensitiveFields = SensitiveFieldsProvider.Get(type);
        var jObject = JObject.Parse(json);

        var fields = jObject.Flatten();

        foreach (var field in sensitiveFields)
        {
            if (fields.TryGetValue(field, out var value))
            {
                fields[field] = options.Value.Getter(value.ToString()!);
            }
            else
            {
                for (int i = 0; i < fields.Keys.Count; i++)
                {
                    var key = fields.Keys.ElementAt(i);
                    var replaced = Regex.Replace(key, "\\[.*?\\]", "", RegexOptions.NonBacktracking);
                    if (replaced == field && fields.TryGetValue(key, out var v))
                    {
                        fields[key] = options.Value.Getter(v.ToString()!);
                    }
                }
            }
        }

        var redactedString = fields.Unflatten().ToString();
        return redactedString;
    }
}
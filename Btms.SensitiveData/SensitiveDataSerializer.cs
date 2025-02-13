using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Btms.Common.Extensions;
using Json.Patch;
using Json.Path;
using Json.Pointer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Btms.SensitiveData;

public class SensitiveDataSerializer(IOptions<SensitiveDataOptions> options, ILogger<SensitiveDataSerializer> logger) : ISensitiveDataSerializer
{
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        TypeInfoResolver = new SensitiveDataTypeInfoResolver(options.Value),
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public T Deserialize<T>(string json, Action<JsonSerializerOptions>? optionsOverride = null)
    {
        var newOptions = jsonOptions;
        if (optionsOverride is not null)
        {
            newOptions = new JsonSerializerOptions
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

    public string RedactRawJson1(string json, Type type)
    {
        if (options.Value.Include)
        {
            return json;
        }
        var sensitiveFields = SensitiveFieldsProvider.Get(type);

        if (!sensitiveFields.Any())
        {
            return json;
        }

        var rootNode = JsonNode.Parse(json);

        foreach (var sensitiveField in sensitiveFields)
        {
            var jsonPath = JsonPath.Parse($"$.{sensitiveField}");
            var result = jsonPath.Evaluate(rootNode);
    
            foreach (var match in result.Matches)
            {
                JsonPatch patch;
                if (match.Value is JsonArray jsonArray)
                {
                    var redactedList = jsonArray.Select(x =>
                    {
                        var redactedValue = options.Value.Getter(x?.GetValue<string>()!);
                        return redactedValue;
                    }).ToJson();

                    patch = new JsonPatch(PatchOperation.Replace(JsonPointer.Parse($"{match.Location!.AsJsonPointer()}"), JsonNode.Parse(redactedList)));
                }
                else
                {
                    var redactedValue = options.Value.Getter(match.Value?.GetValue<string>()!);
                    patch = new JsonPatch(PatchOperation.Replace(JsonPointer.Parse(match.Location!.AsJsonPointer()), redactedValue));
                }


                var patchResult = patch.Apply(rootNode);
                if (patchResult.IsSuccess)
                {
                    rootNode = patchResult.Result;
                }
            }
        }

        return rootNode!.ToJsonString();
    }
    
    public string RedactRawJson(string json, Type type)
    {
        if (options.Value.Include)
        {
            return json;
        }
        var sensitiveFields = SensitiveFieldsProvider.Get(type);

        if (!sensitiveFields.Any())
        {
            return json;
        }

        var rootNode = JsonNode.Parse(json);

        foreach (var sensitiveField in sensitiveFields)
        {
            var jsonPath = JsonPath.Parse($"$.{sensitiveField}");
            var result = jsonPath.Evaluate(rootNode);
    
            foreach (var match in result.Matches)
            {
                JsonPatch patch;
                if (match.Value is JsonArray jsonArray)
                {
                    var redactedList = jsonArray.Select(x =>
                    {
                        var redactedValue = options.Value.Getter(x?.GetValue<string>()!);
                        return redactedValue;
                    }).ToJson();

                    patch = new JsonPatch(PatchOperation.Replace(JsonPointer.Parse($"{match.Location!.AsJsonPointer()}"), JsonNode.Parse(redactedList)));
                }
                else
                {
                    var redactedValue = options.Value.Getter(match.Value?.GetValue<string>()!);
                    patch = new JsonPatch(PatchOperation.Replace(JsonPointer.Parse(match.Location!.AsJsonPointer()), redactedValue));
                }


                var patchResult = patch.Apply(rootNode);
                if (patchResult.IsSuccess)
                {
                    rootNode = patchResult.Result;
                }
            }
        }

        return rootNode!.ToJsonString();
    }
}
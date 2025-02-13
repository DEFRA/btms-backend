using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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

        var jsonPaths = EnumeratePaths(json).ToList();
        var regex = new Regex("\\[\\d\\]", RegexOptions.Compiled, TimeSpan.FromSeconds(2));
        foreach (var path in jsonPaths)
        {
            var pathStripped = regex.Replace(path, "");

            if (sensitiveFields.Contains(pathStripped))
            {
                var jsonPath = JsonPath.Parse($"$.{path}");
                var result = jsonPath.Evaluate(rootNode);

                foreach (var match in result.Matches)
                {
                    var redactedValue = options.Value.Getter(match.Value?.GetValue<string>()!);
                    var patch = new JsonPatch(PatchOperation.Replace(JsonPointer.Parse(match.Location!.AsJsonPointer()),
                        redactedValue));

                    var patchResult = patch.Apply(rootNode);
                    if (patchResult.IsSuccess)
                    {
                        rootNode = patchResult.Result;
                    }
                }
            }
        }

        return rootNode!.ToJsonString(new JsonSerializerOptions() { WriteIndented = true });
    }

   private static IEnumerable<string> EnumeratePaths(string json)
    {
        var doc = JsonDocument.Parse(json).RootElement;
        var queue = new Queue<(string ParentPath, JsonElement element)>();
        queue.Enqueue(("", doc));
        while (queue.Any())
        {
            var (parentPath, element) = queue.Dequeue();
            yield return QueueIterator(element, parentPath, queue);
        }
    }

    private static string QueueIterator(JsonElement element, string parentPath, Queue<(string ParentPath, JsonElement element)> queue)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                parentPath = parentPath == ""
                    ? parentPath
                    : parentPath + ".";
                foreach (var nextEl in element.EnumerateObject())
                {
                    queue.Enqueue(($"{parentPath}{nextEl.Name}", nextEl.Value));
                }
                break;
            case JsonValueKind.Array:
                foreach (var (nextEl, i) in element.EnumerateArray().Select((jsonElement, i) => (jsonElement, i)))
                {
                    queue.Enqueue(($"{parentPath}[{i}]", nextEl));
                }
                break;
            case JsonValueKind.Undefined:
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                return parentPath;
        }
    }
}
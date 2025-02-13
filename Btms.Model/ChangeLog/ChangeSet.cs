using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Btms.Model.Extensions;
using Json.Patch;
using Json.Path;

namespace Btms.Model.ChangeLog;

public class ChangeSet(JsonPatch jsonPatch, JsonNode jsonNodePrevious)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        TypeInfoResolver = new ChangeSetTypeInfoResolver(),
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public JsonPatch JsonPatch { get; } = jsonPatch;

    public JsonNode Previous { get; } = jsonNodePrevious;

    public static ChangeSet CreateChangeSet<T>(T current, T previous)
    {
        var previousNode = JsonNode.Parse(previous.ToJsonString(_jsonOptions));
        var currentNode = JsonNode.Parse(current.ToJsonString(_jsonOptions));
        var diff = previousNode.CreatePatch(currentNode);

        //exclude fields from patch, like _ts, audit entries etc
        var operations = diff.Operations.Where(x => !x.Path.ToString().Contains("_ts"));

        return new ChangeSet(new JsonPatch(operations), previousNode!);
    }

    public T? GetPreviousValue<T>(string path)
    {
        var jp = JsonPath.Parse($"$.{path}");
        var pathResult = jp.Evaluate(Previous);

        if (pathResult.Matches.Any())
        {
            return pathResult.Matches.First().Value.Deserialize<T>();
        }

        return default;
    }
}
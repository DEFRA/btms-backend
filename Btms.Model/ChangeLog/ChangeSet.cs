using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Btms.Model.Extensions;
using Json.Patch;

namespace Btms.Model.ChangeLog;

public class ChangeSet(JsonPatch jsonPatch)
{
    private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
    {
        TypeInfoResolver = new ChangeSetTypeInfoResolver(),
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public JsonPatch JsonPatch { get; } = jsonPatch;

    public static ChangeSet CreateChangeSet<T>(T current, T previous)
    {
        var previousNode = JsonNode.Parse(previous.ToJsonString(jsonOptions));
        var currentNode = JsonNode.Parse(current.ToJsonString(jsonOptions));
        var diff = previousNode.CreatePatch(currentNode);

        //exclude fields from patch, like _ts, audit entries etc
        var operations = diff.Operations.Where(x => !x.Path.ToString().Contains("_ts"));

        return new ChangeSet(new JsonPatch(operations));
    }
}
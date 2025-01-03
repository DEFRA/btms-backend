using System.Text.Json.Nodes;
using FluentAssertions;

namespace TestGenerator.IntegrationTesting.Backend.Extensions;

public static class JsonExtensions
{
    public static string[] GetKeys(this JsonNode node)
    {
        return node.AsObject()
            .Select(x => x.As<KeyValuePair<string, JsonNode>>().Key)
            .ToArray();
    }
}
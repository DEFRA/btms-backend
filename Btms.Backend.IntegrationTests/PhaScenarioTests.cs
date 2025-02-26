using System.Text.Json;
using System.Text.Json.Nodes;
using Btms.Types.Ipaffs;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestDataGenerator.Scenarios.PhaStubs;
using TestGenerator.IntegrationTesting.Backend;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

public class PhaScenarioTests(ITestOutputHelper testOutputHelper) : MultipleScenarioGeneratorBaseTest(testOutputHelper)
{
    [Theory]
    [InlineData(typeof(PhaStubScenarioGenerator))]
    [InlineData(typeof(PhaFinalisationStubScenarioGenerator))]
    public async Task ShouldImportPhaStubScenario(Type generatorType)
    {
        // Enabling data export will intentionally cause the test to fail. 
        // This prevents accidental commits/merges and serves as a warning 
        // against including sensitive data.
        var exportData = false;

        EnsureEnvironmentInitialised(generatorType);

        var expectedImportNotifications = LoadedData
            .Where(l => l.Message is ImportNotification)
            .Select(l => (ImportNotification)l.Message);

        var importedNotifications = Client.AsJsonApiClient().Get("api/import-notifications?page[size]=100");
        var importedNotificationIds = importedNotifications.Data.Select(d => d.Id).ToArray();

        foreach (var expectedImportNotification in expectedImportNotifications)
        {
            importedNotificationIds.Should().Contain(expectedImportNotification.ReferenceNumber);
        }

        if (exportData)
        {
            await ExportData(importedNotifications.Data);
            Assert.Fail("WARNING: Import notifications have been exported. Ensure sensitive data has been redacted");
        }
    }

    private async Task ExportData(List<ResourceObject> importedNotifications)
    {
        const string outputDirectory = "../../../PhaScenarioTestsOutput";
        Directory.CreateDirectory(outputDirectory);

        var relatedMovements = new List<ResourceIdentifierObject>();
        var relatedGoodsMovements = new List<ResourceIdentifierObject>();
        var jsonApiPropertyNames = new[] { "data", "attributes" };
        const string created = "2025-02-21T13:28:39.129Z";
        const string updated = "2025-02-21T13:28:40.129Z";
        const string updatedEntity = "2025-02-21T13:28:41.129Z";
        const string serviceCalled = "2025-02-21T13:28:42.129Z";
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        foreach (var importNotification in importedNotifications)
        {
            var json = await GetDocument($"api/import-notifications/{importNotification.Id}");
            var jsonNode = JsonNode.Parse(json)!;

            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["partOne", "pointOfEntry"]), "GBTEEP1");
            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["created"]), created);
            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["updated"]), updated);
            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["updatedEntity"]), updatedEntity);
            foreach (var node in jsonNode["data"]!["attributes"]!["auditEntries"]!.AsArray())
            {
                AssertPropertyAndUpdate(node!, ["createdLocal"], created);
                AssertPropertyAndUpdate(node!, ["createdSource"], created);
            }

            var jsonString = jsonNode.ToJsonString(jsonOptions);
            await File.WriteAllTextAsync($"{outputDirectory}/btms-import-notification-single-{importNotification.Id}.json", jsonString);

            if (importNotification.Relationships!.TryGetValue("movements", out var movements))
                relatedMovements.AddRange(movements!.Data.ManyValue!);

            if (importNotification.Relationships!.TryGetValue("gmrs", out var goodsMovements))
                relatedGoodsMovements.AddRange(goodsMovements!.Data.ManyValue!);
        }

        var mrns = relatedMovements.Select(r => r.Id).Distinct();
        foreach (var mrn in mrns)
        {
            var json = await GetDocument($"api/movements/{mrn}");
            var jsonNode = JsonNode.Parse(json)!;

            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["created"]), created);
            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["updated"]), updated);
            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["updatedEntity"]), updatedEntity);
            foreach (var node in jsonNode["data"]!["attributes"]!["decisions"]!.AsArray())
            {
                AssertPropertyAndUpdate(node!, ["serviceHeader", "correlationId"], "[see BTMS integration test]");
                AssertPropertyAndUpdate(node!, ["serviceHeader", "serviceCalled"], serviceCalled);
            }
            foreach (var node in jsonNode["data"]!["attributes"]!["auditEntries"]!.AsArray())
            {
                AssertPropertyAndUpdate(node!, ["createdLocal"], created);
                AssertPropertyAndUpdate(node!, ["createdSource"], created);

                if (node!["context"] != null && node["context"]!["importNotifications"] != null)
                {
                    foreach (var node2 in node["context"]!["importNotifications"]!.AsArray())
                    {
                        AssertPropertyAndUpdate(node2!, ["created"], created);
                        AssertPropertyAndUpdate(node2!, ["updated"], updated);
                        AssertPropertyAndUpdate(node2!, ["updatedEntity"], updatedEntity);
                    }
                }
            }

            var jsonString = jsonNode.ToJsonString(jsonOptions);
            await File.WriteAllTextAsync($"{outputDirectory}/btms-movement-single-{mrn}.json", jsonString);
        }

        var grmIds = relatedGoodsMovements.Select(r => r.Id).Distinct();
        foreach (var grmId in grmIds)
        {
            var json = await GetDocument($"api/gmrs/{grmId}");
            var jsonNode = JsonNode.Parse(json)!;

            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["created"]), created);
            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["updated"]), updated);
            AssertPropertyAndUpdate(jsonNode, jsonApiPropertyNames.Concat(["updatedEntity"]), updatedEntity);
            foreach (var node in jsonNode["data"]!["attributes"]!["auditEntries"]!.AsArray())
            {
                AssertPropertyAndUpdate(node!, ["createdLocal"], created);
            }

            var jsonString = jsonNode.ToJsonString(jsonOptions);
            await File.WriteAllTextAsync($"{outputDirectory}/btms-goods-movement-single-{grmId}.json", jsonString);
        }
    }

    private static void AssertPropertyAndUpdate(JsonNode jsonNode, IEnumerable<string> nestedPropertyNames, string value)
    {
        var activeJsonNode = jsonNode;
        var propertyNames = nestedPropertyNames.ToArray();

        foreach (var propertyName in propertyNames.SkipLast(1))
        {
            activeJsonNode![propertyName].Should().NotBeNull();
            activeJsonNode = activeJsonNode[propertyName];
            activeJsonNode.Should().NotBeNull();
        }

        var lastPropertyName = propertyNames[^1];

        activeJsonNode![lastPropertyName].Should().NotBeNull($"Field chain {string.Join(':', propertyNames)} not found");
        activeJsonNode[lastPropertyName] = value;
    }

    private async Task<string> GetDocument(string path)
    {
        var response = await Client.AsHttpClient().GetStringAsync(path);

        var document = JsonDocument.Parse(response);
#pragma warning disable CA1869
        return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
#pragma warning restore CA1869
    }
}
using System.Text.Json;
using System.Text.Json.Nodes;
using Btms.SensitiveData;
using Btms.Types.Ipaffs;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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

        var importedNotifications = Client.AsJsonApiClient().Get("api/import-notifications?page[size]=20");
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

        foreach (var importNotification in importedNotifications)
        {
            var json = await GetDocument($"api/import-notifications/{importNotification.Id}");

            var jsonNode = JsonNode.Parse(json);
            jsonNode!["data"]!["attributes"]!["partOne"]!["pointOfEntry"] = "GBTEEP1";
            var jsonString = jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

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
            await File.WriteAllTextAsync($"{outputDirectory}/btms-movement-single-{mrn}.json", json);
        }

        var grmIds = relatedGoodsMovements.Select(r => r.Id).Distinct();
        foreach (var grmId in grmIds)
        {
            var json = await GetDocument($"api/gmrs/{grmId}");
            await File.WriteAllTextAsync($"{outputDirectory}/btms-goods-movement-single-{grmId}.json", json);
        }


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
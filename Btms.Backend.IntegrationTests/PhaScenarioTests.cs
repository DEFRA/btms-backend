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
    [InlineData(typeof(PhaStubScenarioGenerator), "PhaStub")]
    [InlineData(typeof(PhaFinalisationStubScenarioGenerator), "PhaFinalisationStub")]
    public async Task ShouldImportPhaStubScenario(Type generatorType, string folder)
    {
        var exportData = true;
        var redactData = true;

        if (redactData)
            await RedactIPAFFSFiles(folder);

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
            await ExportData(importedNotifications.Data);

    }

    private async Task ExportData(List<ResourceObject> importedNotifications)
    {
        const string outputDirectory = "../../../PhaScenarioTestsOutput";
        Directory.CreateDirectory(outputDirectory);

        var relatedMovements = new List<ResourceIdentifierObject>();
        var relatedGoodsMovements = new List<ResourceIdentifierObject>();

        foreach (var importNotification in importedNotifications)
        {
            var json = await GetDocument($"api/import-notifications/{importNotification.Id}", Client.AsHttpClient());
        
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
            var json = await GetDocument($"api/movements/{mrn}", Client.AsHttpClient());
            await File.WriteAllTextAsync($"{outputDirectory}/btms-movement-single-{mrn}.json", json);
        }

        var grmIds = relatedGoodsMovements.Select(r => r.Id).Distinct();
        foreach (var grmId in grmIds)
        {
            var json = await GetDocument($"api/gmrs/{grmId}", Client.AsHttpClient());
            await File.WriteAllTextAsync($"{outputDirectory}/btms-goods-movement-single-{grmId}.json", json);
        }
    }

    private async Task<string> GetDocument(string path, HttpClient httpClient)
    {
        var response = await Client.AsHttpClient().GetStringAsync(path);

        var document = JsonDocument.Parse(response);
#pragma warning disable CA1869
        return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
#pragma warning restore CA1869
    }

    private async Task RedactIPAFFSFiles(string scenarioFolder)
    {
        var di = new DirectoryInfo($"../../../../TestDataGenerator/Scenarios/Samples/{scenarioFolder}/IPAFFS");

        var files = di.GetFiles("*.json", SearchOption.AllDirectories);

        await Parallel.ForEachAsync(files, async (fileInfo, ct) =>
        {
            
            var json = await File.ReadAllTextAsync(fileInfo.FullName, ct);
            var options = new SensitiveDataOptions { Include = false };
            var serializer =
                new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance, new SensitiveFieldsProvider());
            var result = serializer.RedactRawJson(json, typeof(ImportNotification));
            await File.WriteAllTextAsync(fileInfo.FullName, result, ct);
        });
    }

}
using System.Security.Authentication;
using System.Text.Json;
using Btms.BlobService;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;
using TestDataGenerator.Scenarios;

namespace TestDataGenerator;

public class Generator(ILogger<Generator> logger, IBlobService blobService)
{
    internal async Task Cleardown(string rootPath)
    {
        await blobService.CleanAsync($"{rootPath}/");
    }

    public async Task Generate(int scenario, ScenarioConfig config, string rootPath)
    {
        int days = config.CreationDateRange;
        int count = config.Count;
        ScenarioGenerator generator = config.Generator;
        
        logger.LogInformation("Generating {Count}x{Days} {Generator}.", count, days, generator);

        for (var d = -days + 1; d <= 0; d++)
        {
            logger.LogInformation("Generating day {D}", d);
            var entryDate = DateTime.Today.AddDays(d);

            for (var i = 0; i < count; i++)
            {
                logger.LogInformation("Generating item {I}", i);

                var generatorResult = generator.Generate(scenario, i, entryDate, config);
                var uploadResult = await InsertToBlobStorage(generatorResult, rootPath);
                if (!uploadResult) throw new AuthenticationException("Error uploading item. Probably auth.");
            }
        }
    }

    private async Task<bool> InsertToBlobStorage(ScenarioGenerator.GeneratorResult result, string rootPath)
    {
        logger.LogInformation(
            "Uploading {ImportNotificationsLength} Notification(s) and {ClearanceRequestsLength} Clearance Request(s) to blob storage",
            result.ImportNotifications.Length, result.ClearanceRequests.Length);

        var importNotificationBlobItems = result.ImportNotifications.Select(n =>
            new BtmsBlobItem { Name = n.BlobPath(rootPath), Content = JsonSerializer.Serialize(n) });

        var alvsClearanceRequestBlobItems = result.ClearanceRequests.Select(cr =>
            new BtmsBlobItem { Name = cr.BlobPath(rootPath), Content = JsonSerializer.Serialize(cr) });

        var success = await blobService.CreateBlobsAsync(importNotificationBlobItems
            .Concat(alvsClearanceRequestBlobItems).ToArray<IBlobItem>());

        return success;
    }
}
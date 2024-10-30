using System.Text.Json;
using Cdms.Types.Alvs;
using Cdms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;
using TestDataGenerator.Services;

namespace TestDataGenerator;

public class Generator(ILogger<Generator> logger, IBlobService blobService)
{
    internal async Task Cleardown()
    {
        var path = DataHelpers.RootBlobPath();
        // await blobService.CleanAsync(path);
    }
    internal async Task Generate(int count, ScenarioGenerator generator)
    {
        logger.LogInformation($"Generating {count}x {generator}.");

        for (int i = 0; i < count; i++)
        {
            logger.LogInformation($"Generating item {i}");

            var result = generator.Generate(i);
            await InsertToBlobStorage(result);
        }
    }

    internal async Task<bool> InsertToBlobStorage(ScenarioGenerator.GeneratorResult result)
    {
        logger.LogInformation($"Uploading {result.ImportNotifications.Length} Notification(s) and {result.ClearanceRequests.Length} Clearance Request(s) to blob storage");

        var importNotificationBlobItems = result.ImportNotifications.Select(n => 
            new BlobItem() { Name = n.BlobPath(), Content = JsonSerializer.Serialize(n) });
        
        var alvsClearanceRequestBlobItems = result.ClearanceRequests.Select(cr => 
            new BlobItem() { Name = cr.BlobPath(), Content = JsonSerializer.Serialize(cr) });

        var success = await blobService.CreateBlobsAsync(importNotificationBlobItems.Concat(alvsClearanceRequestBlobItems).ToArray());
        return success;
    }
}
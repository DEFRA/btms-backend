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
    internal async Task Generate(int count, int days, ScenarioGenerator generator)
    {
        logger.LogInformation($"Generating {count}x{days} {generator}.");

        for (int d = -days+1; d <= 0; d++)
        {
            logger.LogInformation($"Generating day {d}");
            var entryDate = DateTime.Today.AddDays(d);
            
            for (int i = 0; i < count; i++)
            {
                logger.LogInformation($"Generating item {i}");
            
                var result = generator.Generate(i, entryDate);
                await InsertToBlobStorage(result);
            }
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
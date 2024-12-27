using System.Security.Authentication;
using System.Text.Json;
using Btms.BlobService;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;
using TestDataGenerator.Scenarios;

namespace TestDataGenerator;

public class Generator(ILogger<Generator> logger, IBlobService blobService)
{
    public async Task Cleardown(string rootPath)
    {
        await blobService.CleanAsync($"{rootPath}/");
    }

    public async Task<List<(ScenarioGenerator generator, int scenario, int dateOffset, int count, ScenarioGenerator.GeneratorResult result)>> Generate(int scenario, ScenarioConfig config, string rootPath)
    {
        var days = config.CreationDateRange;
        var count = config.Count;
        var generator = config.Generator;
        
        logger.LogInformation("Generating {Count}x{Days} {@Generator}", count, days, generator);
        var output = new List<(ScenarioGenerator, int, int, int, ScenarioGenerator.GeneratorResult result)>();
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
                
                output.Add((generator, scenario, d, i, generatorResult));
            }
        }

        return output;
    }

    private async Task<bool> InsertToBlobStorage(ScenarioGenerator.GeneratorResult result, string rootPath)
    {
        logger.LogInformation(
            "Uploading {Count} messages to blob storage",
            result.Count);
        
        var blobs = result.Select(r => new BtmsBlobItem
        {
            Name = r.BlobPath(rootPath), Content = JsonSerializer.Serialize(r)
        });

        var success = await blobService.CreateBlobsAsync(blobs.ToArray<IBlobItem>());
        
        // var success = await blobService.CreateBlobsAsync(importNotificationBlobItems
        //     .Concat(alvsClearanceRequestBlobItems).ToArray<IBlobItem>());

        return success;
    }
}
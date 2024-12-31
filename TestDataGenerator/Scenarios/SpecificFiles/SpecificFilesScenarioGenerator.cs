using System.Text.RegularExpressions;
using Btms.BlobService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public abstract class SpecificFilesScenarioGenerator(IServiceProvider sp, ILogger logger) : ScenarioGenerator
{
    private readonly IBlobService blobService = sp.GetRequiredService<CachingBlobService>();
    private readonly Regex regex = new (@"(202\d\/\d{2}\/\d{2})");
    
    internal async Task<List<(string filePath, DateTime created, IBaseBuilder builder)>> GetBuilders(string scenarioPath)
    {
        var tokenSource = new CancellationTokenSource();
        var clearanceRequestBlobs = blobService.GetResourcesAsync($"{scenarioPath}/ALVS", tokenSource.Token);

        var clearanceRequestList = await GetBuildersForFolder($"{scenarioPath}/ALVS", GetClearanceRequestBuilder, tokenSource.Token);
        var notificationList = await GetBuildersForFolder($"{scenarioPath}/IPAFFS", GetNotificationBuilder, tokenSource.Token);
        var decisionList = await GetBuildersForFolder($"{scenarioPath}/DECISIONS", GetNotificationBuilder, tokenSource.Token);

        return clearanceRequestList
            .Concat(notificationList)
            .Concat(decisionList)
            .OrderBy(b => b.created)
            .ToList();
    }

    private async Task<List<(string file, DateTime created, IBaseBuilder builder)>> GetBuildersForFolder(string scenarioFolder, Func<string,  string, IBaseBuilder> createBuilder, CancellationToken token)
    {
        var blobs = blobService.GetResourcesAsync(scenarioFolder, token);

        var list = new List<(string, DateTime, IBaseBuilder)>();
        
        await foreach (var blobItem in blobs)
        {
            var m = regex.Match(blobItem.Name);

            if (!m.Success) {
                throw new IOException("File path doesn't include a date in the expected format");
            }
            
            logger.LogInformation("Found blob item {name}", blobItem.Name);
            var builder = createBuilder(blobItem.Name, "");
            list.Add((blobItem.Name, builder.Created!.Value, builder!));
        }

        return list;
    }
    
}
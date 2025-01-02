using System.Text.RegularExpressions;
using Btms.BlobService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public abstract class SpecificFilesScenarioGenerator(IServiceProvider sp, ILogger logger) : ScenarioGenerator
{
    private readonly IBlobService blobService = sp.GetRequiredService<CachingBlobService>();
    
    internal async Task<List<(string filePath, IBaseBuilder builder)>> GetBuilders(string scenarioPath)
    {
        var tokenSource = new CancellationTokenSource();
        var clearanceRequestBlobs = blobService.GetResourcesAsync($"{scenarioPath}/ALVS", tokenSource.Token);

        var clearanceRequestList = await GetBuildersForFolder($"{scenarioPath}/ALVS", GetClearanceRequestBuilder, tokenSource.Token);
        var notificationList = await GetBuildersForFolder($"{scenarioPath}/IPAFFS", GetNotificationBuilder, tokenSource.Token);
        var decisionList = await GetBuildersForFolder($"{scenarioPath}/DECISIONS", GetDecisionBuilder, tokenSource.Token);

        return clearanceRequestList
            .Concat(notificationList)
            .Concat(decisionList)
            .ToList();
    }

    private async Task<List<(string file, IBaseBuilder builder)>> GetBuildersForFolder(string scenarioFolder, Func<string, string, IBaseBuilder> createBuilder, CancellationToken token)
    {
        var blobs = blobService.GetResourcesAsync(scenarioFolder, token);

        var list = new List<(string, IBaseBuilder)>();
        
        await foreach (var blobItem in blobs)
        {   
            logger.LogInformation("Found blob item {name}", blobItem.Name);
            var builder = createBuilder(blobItem.Name, "");
            list.Add((blobItem.Name, builder!));
        }

        return list;
    }
    
}
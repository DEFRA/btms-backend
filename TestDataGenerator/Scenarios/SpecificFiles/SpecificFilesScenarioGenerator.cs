using System.Text.RegularExpressions;
using Btms.BlobService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public abstract class SpecificFilesScenarioGenerator(IServiceProvider sp, ILogger logger) : ScenarioGenerator
{
    private IBlobService blobService = sp.GetRequiredService<CachingBlobService>();
    private Regex regex = new (@"(202\d\/\d{2}\/\d{2})");
    
    internal async Task<List<(string filePath, DateTime created, IBaseBuilder builder)>> GetBuilders(string path)
    {
        var tokenSource = new CancellationTokenSource();
        var clearanceRequestBlobs = blobService.GetResourcesAsync($"{path}/ALVS", tokenSource.Token);

        var clearanceRequestList = await GetBuildersForFolder($"{path}/ALVS", GetClearanceRequestBuilder, tokenSource.Token);
        // var notificationList = await GetBuildersForFolder($"{path}/IPAFFS", GetNotificationBuilder, tokenSource.Token);
        // var decisionList = await GetBuildersForFolder($"{path}/DECISIONS", GetNotificationBuilder, tokenSource.Token);

        return clearanceRequestList
            // .Concat(notificationList)
            // .Concat(decisionList)
            .OrderBy(b => b.created)
            .ToList();
    }

    private async Task<List<(string file, DateTime created, IBaseBuilder builder)>> GetBuildersForFolder(string folder, Func<string,  string, IBaseBuilder> createBuilder, CancellationToken token)
    {
        var blobs = blobService.GetResourcesAsync(folder, token);

        var list = new List<(string, DateTime, IBaseBuilder)>();
        
        await foreach (var blobItem in blobs)
        {
            var m = regex.Match(blobItem.Name);

            if (!m.Success) {
                throw new IOException("File path doesn't include a date in the expected format");
            }
            
            // String part1 = m.Groups[1].Value;
            // String part2 = m.Groups[2].Value;
            // return true;
            logger.LogInformation("Found blob item {name}", blobItem.Name);
            var builder = createBuilder(blobItem.Name, "");
            list.Add((blobItem.Name, builder.Created!.Value, builder!));
        }

        return list;
    }
    
}
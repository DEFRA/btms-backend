using System.Text.RegularExpressions;
using Btms.BlobService;
using Btms.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class Mrn24GBDEEA43OY1CQAR7ScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDEEA43OY1CQAR7ScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "Mrn-24GBDEEA43OY1CQAR7");

public class Mrn24GBDEHMFC4WGXVAR7ScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDEHMFC4WGXVAR7ScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "Mrn-24GBDEHMFC4WGXVAR7");

public class Mrn24GBDDJER3ZFRMZAR9ScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDDJER3ZFRMZAR9ScenarioGenerator> logger) : SpecificFilesScenarioGenerator(
    sp, logger, "Mrn-24GBDDJER3ZFRMZAR9");

public class Mrn24GBDE3CF94H96TAR0ScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDE3CF94H96TAR0ScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "Mrn-24GBDE3CF94H96TAR0");

public class Mrn24GBDYHI8LMFLDQAR6ScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDYHI8LMFLDQAR6ScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "Mrn-24GBDYHI8LMFLDQAR6");

public class Mrn24GBDPN81VSULAGAR9ScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDPN81VSULAGAR9ScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "Mrn-24GBDPN81VSULAGAR9");

public class ChedPpPhsiDecisionTestsScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDPN81VSULAGAR9ScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "ChedPpPhsiDecisionTests");

public class ChedPpHmiDecisionTestsScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDPN81VSULAGAR9ScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "ChedPpHmiDecisionTests");

public class DeletedNotificationTestsScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDPN81VSULAGAR9ScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "DeletedNotification");

public class CancelledNotificationTestsScenarioGenerator(
    IServiceProvider sp,
    ILogger<Mrn24GBDPN81VSULAGAR9ScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "CancelledNotification");

public class DuplicateMovementItems_CDMS_211(IServiceProvider sp, ILogger<DuplicateMovementItems_CDMS_211> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "DuplicateMovementItems-CDMS-211");

public abstract class SpecificFilesScenarioGenerator(IServiceProvider sp, ILogger logger, string? sampleFolder = null) : ScenarioGenerator
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
    
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        if (!sampleFolder.HasValue())
        {
            throw new InvalidOperationException(
                "Either need to specify the scenarioPath in the constructor, or ovrride the generate function.");
        }
        var builders =  GetBuilders(sampleFolder)
            .GetAwaiter().GetResult();
        
        logger.LogInformation("Created {builders} Builders", 
            builders.Count);

        var messages = builders
            .Select(b => b.builder)
            .ToArray()
            .BuildAll()
            .ToArray();
        
        return new GeneratorResult(messages);
    }
    
}
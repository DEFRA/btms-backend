using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public abstract class DatasetSpecificFilesScenarioGenerator(IServiceProvider sp, ILogger<DatasetSpecificFilesScenarioGenerator> logger, string? sampleFolder = null) : SpecificFilesScenarioGenerator(sp, logger, sampleFolder)
{
    protected override List<(string filePath, IBaseBuilder builder)> ModifyBuilders(List<(string filePath, IBaseBuilder builder)> builders, int? scenario = null, int? item = null, DateTime? entryDate = null)
    {
        if (!scenario.HasValue || !item.HasValue || !entryDate.HasValue)
        {
            return builders;
        }

        // There may be multiple CHEDs and MRNs in the list of builders, we want to keep those that
        // are the same, the same, but change them to something thats unique...
        Dictionary<string, string> referenceMap = new Dictionary<string, string>();

        builders.ForEach(b =>
            {
                switch (b.builder)
                {
                    case ImportNotificationBuilder i:
                        //Todo, need ched type...
                        var chedReference = DataHelpers.GenerateReferenceNumber(ImportNotificationTypeEnum.Chedpp, scenario.Value, DateTime.Today, item.Value);

                        break;
                    case ClearanceRequestBuilder c:
                        break;
                    default:
                        break;
                }
            });


        // .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)

        return builders;
    }
}
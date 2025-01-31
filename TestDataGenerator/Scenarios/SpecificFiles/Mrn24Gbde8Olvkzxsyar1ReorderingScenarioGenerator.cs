using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class Mrn24Gbde8Olvkzxsyar1ImportNotificationsAtEndScenarioGenerator(IServiceProvider sp, ILogger<Mrn24Gbde8Olvkzxsyar1ImportNotificationsAtEndScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger, "Mrn-24GBDE8OLVKZXSYAR1")
{
    private class PutImportNotificationsAtEndComparer : IComparer<object>
    {
        public int Compare(object? x, object? y)
        {
            if (x is ImportNotification && y is ImportNotification)
                return x!.CreatedDate().CompareTo(y!.CreatedDate());
            else if (y is ImportNotification)
                return -1;
            else
                return x!.CreatedDate().CompareTo(y!.CreatedDate());
        }
    }
    
    // protected override List<(string filePath, IBaseBuilder builder)> ModifyBuilders(List<(string filePath, IBaseBuilder builder)> builders)
    // {
    //     builders.Order(new PutImportNotificationsAtEndComparer());
    //     
    //     return builders;
    // }

    protected override List<object> ModifyMessages(List<object> messages)
    {
        return messages
            .Order(new PutImportNotificationsAtEndComparer())
            .ToList();
    }

    // public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    // {
    //     var builders =  GetBuilders("Mrn-24GBDEJ9V2OD0BHAR0").GetAwaiter().GetResult();
    //     
    //     logger.LogInformation("Created {Builders} Builders", 
    //         builders.Count);
    //     
    //     // Set the manual action of the builders to "Y"
    //     foreach (var builderItem in builders
    //                  .Where(b => b.builder is FinalisationBuilder))
    //     {
    //         ((FinalisationBuilder)builderItem.builder)
    //             .WithManualAction(true);
    //     }
    //     
    //     var messages = builders
    //         .Select(b => b.builder)
    //         .ToArray()
    //         .BuildAll()
    //         .ToArray();
    //     
    //     return new GeneratorResult(messages);
    // }
}
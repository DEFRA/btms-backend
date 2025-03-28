using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class Mrn24Gbdej9V2Od0Bhar0ManualActionScenarioGenerator(IServiceProvider sp, ILogger<Mrn24Gbdej9V2Od0Bhar0ManualActionScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders = GetBuilders("Mrn-24GBDEJ9V2OD0BHAR0").GetAwaiter().GetResult();

        logger.LogInformation("Created {Builders} Builders",
            builders.Count);

        // Set the manual action of the builders to "Y"
        foreach (var builderItem in builders
                     .Where(b => b.builder is FinalisationBuilder))
        {
            ((FinalisationBuilder)builderItem.builder)
                .WithManualAction(true);
        }

        var messages = builders
            .Select(b => b.builder)
            .ToArray()
            .BuildAll()
            .ToArray();

        return new GeneratorResult(messages);
    }
}
using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class Mrn24Gbdej9V2Od0Bhar0DestroyedScenarioGenerator(IServiceProvider sp, ILogger<Mrn24Gbdej9V2Od0Bhar0DestroyedScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders =  GetBuilders("Mrn-24GBDEJ9V2OD0BHAR0").GetAwaiter().GetResult();
        
        logger.LogInformation("Created {builders} Builders", 
            builders.Count);
        
        // Set the final state of the finalisations to Destroyed (3)
        foreach (var builderItem in builders
                     .Where(b => b.builder is FinalisationBuilder))
        {
            ((FinalisationBuilder)builderItem.builder)
                .WithFinalState(3);
        }
        
        var messages = builders
            .Select(b => b.builder)
            .ToArray()
            .BuildAll()
            .ToArray();
        
        return new GeneratorResult(messages);
    }
}
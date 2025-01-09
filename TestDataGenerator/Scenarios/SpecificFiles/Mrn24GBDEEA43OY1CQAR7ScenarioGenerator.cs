using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class Mrn24GBDEEA43OY1CQAR7ScenarioGenerator(IServiceProvider sp, ILogger<Mrn24GBDEEA43OY1CQAR7ScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders =  GetBuilders("Mrn-24GBDEEA43OY1CQAR7").GetAwaiter().GetResult();
        
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
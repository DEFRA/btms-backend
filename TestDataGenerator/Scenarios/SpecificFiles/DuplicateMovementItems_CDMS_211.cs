using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class DuplicateMovementItems_CDMS_211(IServiceProvider sp, ILogger<DuplicateMovementItems_CDMS_211> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders =  GetBuilders("DuplicateMovementItems-CDMS-211").GetAwaiter().GetResult();
        
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
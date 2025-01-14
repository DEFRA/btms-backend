using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class MultiChedPMatchScenarioGenerator(IServiceProvider sp, ILogger<MultiChedPMatchScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders =  GetBuilders("MultiChedWithDecision/ChedP").GetAwaiter().GetResult();
        
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

public class MultiChedAMatchScenarioGenerator(IServiceProvider sp, ILogger<MultiChedAMatchScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders =  GetBuilders("MultiChedWithDecision/ChedA").GetAwaiter().GetResult();
        
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

public class MultiChedDMatchScenarioGenerator(IServiceProvider sp, ILogger<MultiChedDMatchScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders =  GetBuilders("MultiChedWithDecision/ChedD").GetAwaiter().GetResult();
        
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
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public abstract class MultiChedMatchScenarioGenerator(
    IServiceProvider sp,
    ILogger<MultiChedMatchScenarioGenerator> logger,
    string scenarioPath) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders = GetBuilders(scenarioPath).GetAwaiter().GetResult();

        var messages = builders
            .Select(b => b.builder)
            .ToArray()
            .BuildAll()
            .ToArray();

        return new GeneratorResult(messages);
    }
}

public class MultiChedPMatchScenarioGenerator(IServiceProvider sp, ILogger<MultiChedPMatchScenarioGenerator> logger)
    : MultiChedMatchScenarioGenerator(sp, logger, "MultiChedWithDecision/ChedP");


public class MultiChedAMatchScenarioGenerator(IServiceProvider sp, ILogger<MultiChedAMatchScenarioGenerator> logger)
    : MultiChedMatchScenarioGenerator(sp, logger, "MultiChedWithDecision/ChedA");

public class MultiChedDMatchScenarioGenerator(IServiceProvider sp, ILogger<MultiChedDMatchScenarioGenerator> logger)
    : MultiChedMatchScenarioGenerator(sp, logger, "MultiChedWithDecision/ChedD");

public class MultiChedPWorstCaseMatchScenarioGenerator(
    IServiceProvider sp,
    ILogger<MultiChedPWorstCaseMatchScenarioGenerator> logger)
    : MultiChedMatchScenarioGenerator(sp, logger, "MultiChedWorstCaseDecision/ChedP")
{

}


public class MultiChedAWorstCaseMatchScenarioGenerator(IServiceProvider sp, ILogger<MultiChedAWorstCaseMatchScenarioGenerator> logger)
    : MultiChedMatchScenarioGenerator(sp, logger, "MultiChedWorstCaseDecision/ChedA");

public class MultiChedDWorstCaseMatchScenarioGenerator(IServiceProvider sp, ILogger<MultiChedDWorstCaseMatchScenarioGenerator> logger)
    : MultiChedMatchScenarioGenerator(sp, logger, "MultiChedWorstCaseDecision/ChedD");
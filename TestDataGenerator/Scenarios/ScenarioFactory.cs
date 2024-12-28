using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios;

public class ScenarioConfig
{
    public required string Name { get; init; }
    public required int Count { get; init; }
    public required int CreationDateRange { get; init; }
    public required int ArrivalDateRange { get; init; } 
    public required ScenarioGenerator Generator { get; init; }
    
    public ScenarioGenerator.GeneratorResult[] Generate(ILogger logger, int scenarioIndex)
    {
        var days = this.CreationDateRange;
        var count = this.Count;
        var generator = this.Generator;
        
        logger.LogInformation("Generating {Count}x{Days} {@Generator}", count, days, generator);
        var results = new List<ScenarioGenerator.GeneratorResult>();
        
        for (var d = -days + 1; d <= 0; d++)
        {
            logger.LogInformation("Generating day {D}", d);
            var entryDate = DateTime.Today.AddDays(d);

            for (var i = 0; i < count; i++)
            {
                logger.LogInformation("Generating item {I}", i);

                results.Add(generator.Generate(scenarioIndex, i, entryDate, this));
            }
        }

        return results.ToArray();
    }
}

public static class ScenarioFactory
{   
    public static ScenarioConfig CreateScenarioConfig<T>(this IServiceProvider sp, int count, int creationDateRange, int arrivalDateRange = 30)
        where T : ScenarioGenerator
    {
        if (count > 100000)
            throw new ArgumentException(
                "Currently only deals with max 100,000 items. Check ImportNotificationBuilder WithReferenceNumber.");

        var scenario = sp.GetRequiredService<T>();
        return new ScenarioConfig
        {
            Name = nameof(T).Replace("ScenarioGenerator", ""), Count = count, CreationDateRange = creationDateRange, ArrivalDateRange = arrivalDateRange, Generator = scenario
        };
    }

    public static ScenarioConfig CreateScenarioConfig<T>(T scenario, int count, int creationDateRange, int arrivalDateRange = 30)
        where T : ScenarioGenerator
    {
        if (count > 100000)
            throw new ArgumentException(
                "Currently only deals with max 100,000 items. Check ImportNotificationBuilder WithReferenceNumber.");

        return new ScenarioConfig
        {
            Name = nameof(T).Replace("ScenarioGenerator", ""),
            Count = count,
            CreationDateRange = creationDateRange,
            ArrivalDateRange = arrivalDateRange,
            Generator = scenario
        };
    }
}
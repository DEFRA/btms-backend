using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestDataGenerator.Scenarios;

public class ScenarioConfig
{
    public string Name { get; set; }
    public int Count { get; set; }
    public int Days { get; set; }
    public ScenarioGenerator Generator { get; set; }
}

public static class ScenarioFactory
{
    public static ScenarioConfig CreateScenarioConfig<T>(this IHost app, int count, int days) where T : ScenarioGenerator
    {
        var scenario = (ScenarioGenerator)app.Services.GetRequiredService<T>();
        return new ScenarioConfig()
        {
            Name = "ChedASimpleMatch",
            Count = 3,
            Days = 7,
            Generator = scenario
        };
    }
}
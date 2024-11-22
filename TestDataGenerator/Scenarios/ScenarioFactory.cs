using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestDataGenerator.Scenarios;

public class ScenarioConfig
{
    public required string Name { get; init; }
    public required int Count { get; init; }
    public required int Days { get; init; }
    public required ScenarioGenerator Generator { get; init; }
}

public static class ScenarioFactory
{
    public static ScenarioConfig CreateScenarioConfig<T>(this IHost app, int count, int days)
        where T : ScenarioGenerator
    {
        if (count > 999999)
            throw new ArgumentException(
                "Currently only deals with max 100,000 items. Check ImportNotificationBuilder WithReferenceNumber.");

        var scenario = app.Services.GetRequiredService<T>();
        return new ScenarioConfig
        {
            Name = nameof(T).Replace("ScenarioGenerator", ""), Count = count, Days = days, Generator = scenario
        };
    }
}
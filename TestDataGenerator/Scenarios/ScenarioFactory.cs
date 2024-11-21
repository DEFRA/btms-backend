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
    public static ScenarioConfig CreateScenarioConfig<T>(this IHost app, int count, int days)
        where T : ScenarioGenerator
    {
        if (count > 999999)
            throw new ArgumentException(
                "Currently only deals with max 100,000 items. Check ImportNotificationBuilder WithReferenceNumber.");

        var scenario = (ScenarioGenerator)app.Services.GetRequiredService<T>();
        return new ScenarioConfig { Name = "ChedASimpleMatch", Count = count, Days = days, Generator = scenario };
    }
}
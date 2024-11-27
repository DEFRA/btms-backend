using Cdms.BlobService;
using Cdms.BlobService.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TestDataGenerator.Config;

namespace TestDataGenerator.Scenarios;

public class ScenarioConfig
{
    public required string Name { get; init; }
    public required int Count { get; init; }
    public required int CreationDateRange { get; init; }
    public required int ArrivalDateRange { get; init; } 
    public required ScenarioGenerator Generator { get; init; }
}

public static class ScenarioFactory
{
    public static ScenarioConfig CreateScenarioConfig<T>(this IHost app, int count, int creationDateRange, int arrivalDateRange = 30)
        where T : ScenarioGenerator
    {
        if (count > 999999)
            throw new ArgumentException(
                "Currently only deals with max 100,000 items. Check ImportNotificationBuilder WithReferenceNumber.");

        var scenario = app.Services.GetRequiredService<T>();
        return new ScenarioConfig
        {
            Name = nameof(T).Replace("ScenarioGenerator", ""), Count = count, CreationDateRange = creationDateRange, ArrivalDateRange = arrivalDateRange, Generator = scenario
        };
    }
}
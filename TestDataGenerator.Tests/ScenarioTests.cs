using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TestDataGenerator.Extensions;
using TestDataGenerator.Scenarios;
using Xunit;

namespace TestDataGenerator.Tests;

public class ScenarioTests
{
    readonly ServiceProvider _serviceProvider = BuilderExtensions.GetDefaultServiceProvider();

    public static IEnumerable<object[]> GetAllScenarios()
    {
        return BuilderExtensions
            .GetAllScenarios()
            .Select(s => new object[] { s });
    }

    [Theory]
    [MemberData(nameof(GetAllScenarios))]
    public void EnsureAllScenarioDefaultsAreValid(Type scenarioType)
    {
        var scenarioTypes = BuilderExtensions.GetAllScenarios();

        var scenario = (ScenarioGenerator)_serviceProvider.GetRequiredService(scenarioType);
        var config = new ScenarioConfig()
        {
            Name = "Test",
            ArrivalDateRange = 1,
            Count = 1,
            Generator = scenario,
            CreationDateRange = 1
        };

        var messages = scenario.Generate(1, 1, DateTime.Today, config);

        messages.Should().NotBeNull();
        messages.Should().NotBeEmpty();
    }
}
using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;

namespace Btms.Analytics.Tests;

[Trait("Category", "Integration")]
public class MovementsExceptionsTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    static readonly List<object[]> scenarios = [
        [typeof(Mrn24GBD46KPSVZ3DFAR2ExceptionEndScenarioGenerator), "HasChedppChecks FeatureMissing : Cdms205Ac5"],
        [typeof(CrNoMatchSingleItemWithDecisionScenarioGenerator), "Alvs has match decisions but no Btms links"]
    ];

    public static IEnumerable<object[]> Scenarios()
    {
        foreach (var scenario in scenarios)
        {
            yield return [scenario[0]];
        }
    }

    public static IEnumerable<object[]> ScenariosWithException()
    {
        foreach (var scenario in scenarios)
        {
            yield return [scenario[0], scenario[1]];
        }
    }

    private async Task<SingleSeriesDataset> EnsureEnvironmentInitialisedAndGetExceptions(Type generatorType)
    {
        EnsureEnvironmentInitialised(generatorType);
        
        var result = await Client
            .GetAnalyticsDashboard(["movementsExceptions"],
                dateFrom: DateTime.MinValue, dateTo: DateTime.MaxValue);
         
        var chart = await result
            .AnalyticsChartAs<SingleSeriesDataset>("movementsExceptions")!;

        return chart;
    }
    
    [Theory]
    [MemberData(nameof(Scenarios))]
    public async Task ShouldHaveAnException(Type generatorType)
    {
        var chart = await EnsureEnvironmentInitialisedAndGetExceptions(generatorType);
         
         chart.Values.Count
             .Should().BeGreaterThan(1);
    }
    
    
    [Theory]
    [MemberData(nameof(ScenariosWithException))]
    public async Task ShouldHaveCorrectException(Type generatorType, string expectedException)
    {
        var chart = await EnsureEnvironmentInitialisedAndGetExceptions(generatorType);

         chart.Values
             .Single(v => v.Key == expectedException)
             .Value
             .Should().Be(1);
         }
}
using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;

namespace Btms.Analytics.Tests;

public class MovementsExceptionsTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrNoMatchSingleItemWithDecisionScenarioGenerator>(output)
{
    
    [Fact] //(Skip="Change to an individiual scenario test that generates an exception. Possibly a seperate test for each exception type")]
    public async Task WhenCalled_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        
        var result = await Client
            .GetAnalyticsDashboard(["movementsExceptions"],
                dateFrom: DateTime.MinValue, dateTo: DateTime.MaxValue);
        
        var chart = await result
            .AnalyticsChartAs<SingleSeriesDataset>("movementsExceptions")!;
        
        chart.Values.Count
            .Should().BeGreaterThan(1);
        
        chart.Values
            .Single(v => v.Key == "Alvs has match decisions but no Btms links")
            .Value
            .Should().Be(1);
        
    }
}
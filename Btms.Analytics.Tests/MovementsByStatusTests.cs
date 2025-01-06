using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class MovementsByStatusTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetMovementsAggregationService()
            .ByStatus(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()));

        TestOutputHelper.WriteLine("{0} aggregated items found", result.Values.Count);
        
        result.Values.Count.Should().Be(3);
        result.Values.Keys.Order().Should().Equal("Investigate", "Linked", "Not Linked");
    }
    
    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetMovementsAggregationService()
            .ByStatus(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour()));

        TestOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(3);
        result.Values.Keys.Order().Should().Equal("Investigate", "Linked", "Not Linked");
    }
    
    [Fact]
    public async Task WhenCalledWithTimePeriodYieldingNoResults_ReturnEmptyAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetMovementsAggregationService()
            .ByStatus(DateTime.MaxValue.AddDays(-1), DateTime.MaxValue));

        TestOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(3);
        result.Values.Keys.Order().Should().Equal("Investigate", "Linked", "Not Linked");
    }
}
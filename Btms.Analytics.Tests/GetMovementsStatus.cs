using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Analytics.Tests;

[Collection("Aggregation Test collection")]
public class GetMovementsStatus(
    AggregationTestFixture aggregationTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await aggregationTestFixture.MovementsAggregationService
            .ByStatus(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()));

        testOutputHelper.WriteLine("{0} aggregated items found", result.Values.Count);
        
        result.Values.Count.Should().Be(2);
        result.Values.Keys.Order().Should().Equal("Linked", "Not Linked");
    }
    
    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await aggregationTestFixture.MovementsAggregationService
            .ByStatus(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour()));

        testOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(2);
        result.Values.Keys.Order().Should().Equal("Linked", "Not Linked");
    }
    
    [Fact]
    public async Task WhenCalledWithTimePeriodYieldingNoResults_ReturnEmptyAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await aggregationTestFixture.MovementsAggregationService
            .ByStatus(DateTime.MaxValue.AddDays(-1), DateTime.MaxValue));

        testOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(2);
        result.Values.Keys.Order().Should().Equal("Linked", "Not Linked");
    }
}
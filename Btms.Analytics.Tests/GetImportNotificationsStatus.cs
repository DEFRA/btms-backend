using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Analytics.Tests;

[Collection("Aggregation Test collection")]
public class GetImportNotificationsStatus(
    AggregationTestFixture aggregationTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await aggregationTestFixture.ImportNotificationsAggregationService
            .ByStatus(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()));

        testOutputHelper.WriteLine("{0} aggregated items found", result.Values.Count);
        
        result.Values.Count.Should().Be(8);
    }
    
    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await aggregationTestFixture.ImportNotificationsAggregationService
            .ByStatus(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour()));

        testOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(8);
        result.Values.Keys.Order().Should().Equal(
            "CHEDA Linked", "CHEDA Not Linked", "CHEDD Linked", "CHEDD Not Linked", "CHEDP Linked", "CHEDP Not Linked", "CHEDPP Linked", "CHEDPP Not Linked");

    }
    
    [Fact]
    public async Task WhenCalledWithTimePeriodYieldingNoResults_ReturnEmptyAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await aggregationTestFixture.ImportNotificationsAggregationService
            .ByStatus(DateTime.MaxValue.AddDays(-1), DateTime.MaxValue));

        testOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(8);
    }
}
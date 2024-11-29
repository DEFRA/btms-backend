using Cdms.Common.Extensions;
using Cdms.Model.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Cdms.Analytics.Tests;

[Collection("Aggregation Test collection")]
public class GetMovementsByCreatedDateTests(
    AggregationTestFixture aggregationTestFixture,
    ITestOutputHelper testOutputHelper) {
    
    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        var result = (await aggregationTestFixture.LinkingAggregationService
            .MovementsByCreated(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(), AggregationPeriod.Hour))
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(2);

        result[0].Name.Should().Be("Linked");
        result[0].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today);
        result[0].Periods.Count.Should().Be(48);
        
        result[1].Name.Should().Be("Not Linked");
    }
    
    [Fact]
    public async Task WhenCalledLastMonth_ReturnExpectedAggregation()
    {
        var result = (await aggregationTestFixture.LinkingAggregationService
            .MovementsByCreated(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()))
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(2);

        result[0].Name.Should().Be("Linked");
        result[0].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today);
        result[0].Periods.Count.Should().Be(DateTime.Today.DaysSinceMonthAgo() + 1);
        
        result[1].Name.Should().Be("Not Linked");
    }
}
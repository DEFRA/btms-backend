using Btms.Analytics;
using Btms.Common.Extensions;
using Btms.Model.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Analytics.Tests;

[Collection("Aggregation Test collection")]
public class GetMovementsByArrivalDateTests(
    AggregationTestFixture aggregationTestFixture,
    ITestOutputHelper testOutputHelper) {
    
    [Fact]
    public async Task WhenCalledNext72Hours_ReturnExpectedAggregation()
    {

        var result = (await aggregationTestFixture.MovementsAggregationService
            .ByArrival(DateTime.Now.CurrentHour(), DateTime.Now.CurrentHour().AddDays(3), AggregationPeriod.Hour))
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Select(r => r.Name).Order().Should().Equal("Linked", "Not Linked");

        result[0].Periods[0].Period.Should().BeOnOrAfter(DateTime.Today);
        result[0].Periods.Count.Should().Be(72);
    }
    
    [Fact]
    public async Task WhenCalledNextMonth_ReturnExpectedAggregation()
    {
        var result = (await aggregationTestFixture.MovementsAggregationService
            .ByArrival(DateTime.Today, DateTime.Today.MonthLater()))
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(2);

        result[0].Name.Should().Be("Linked");
        result[0].Periods[0].Period.Should().BeOnOrAfter(DateTime.Today);
        result[0].Periods.Count.Should().Be(DateTime.Today.DaysUntilMonthLater());
        
        result[1].Name.Should().Be("Not Linked");
    }
}
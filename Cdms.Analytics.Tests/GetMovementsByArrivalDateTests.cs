using Cdms.Common.Extensions;
using Cdms.Model.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Cdms.Analytics.Tests;

[Collection("Aggregation Test collection")]
public class GetMovementsByArrivalDateTests(
    AggregationTestFixture aggregationTestFixture,
    ITestOutputHelper testOutputHelper) {
    
    [Fact]
    public async Task WhenCalledNext24Hours_ReturnExpectedAggregation()
    {

        var result = (await aggregationTestFixture.LinkingAggregationService
            .MovementsByArrival(DateTime.Now, DateTime.Now.Tomorrow(), AggregationPeriod.Hour))
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(2);

        result[0].Name.Should().Be("Linked");
        result[0].Periods[0].Period.Should().BeOnOrAfter(DateTime.Today);
        result[0].Periods.Count.Should().Be(24);
        
        result[1].Name.Should().Be("Not Linked");
    }
    
    [Fact]
    public async Task WhenCalledNextMonth_ReturnExpectedAggregation()
    {
        var result = (await aggregationTestFixture.LinkingAggregationService
            .MovementsByArrival(DateTime.Today, DateTime.Today.MonthLater()))
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(2);

        result[0].Name.Should().Be("Linked");
        result[0].Periods[0].Period.Should().BeOnOrAfter(DateTime.Today);
        result[0].Periods.Count.Should().Be(DateTime.Today.DaysUntilMonthLater());
        
        result[1].Name.Should().Be("Not Linked");
    }
}
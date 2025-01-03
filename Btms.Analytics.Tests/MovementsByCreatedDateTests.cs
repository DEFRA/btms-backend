using Btms.Common.Extensions;
using Btms.Model.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Btms.Analytics.Tests.Fixtures;

namespace Btms.Analytics.Tests;

[Collection(nameof(BasicSampleDataTestCollection))]
public class MovementsByCreatedDateTests(
    BasicSampleDataTestFixture basicSampleDataTestFixture,
    ITestOutputHelper testOutputHelper) 
{
    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        var result = (await basicSampleDataTestFixture.GetMovementsAggregationService(testOutputHelper)
            .ByCreated(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(), AggregationPeriod.Hour))
            .Series
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(3);

        result[1].Name.Should().Be("Linked");
        result[1].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today);
        result[1].Periods.Count.Should().Be(48);
        
        result[2].Name.Should().Be("Not Linked");
    }
    
    [Fact]
    public async Task WhenCalledWithTimePeriodYieldingNoResults_ReturnEmptyAggregation()
    {
        var from = DateTime.MaxValue.AddDays(-1);
        var to = DateTime.MaxValue;

        var result = (await basicSampleDataTestFixture.GetMovementsAggregationService(testOutputHelper)
            .ByCreated(from, to, AggregationPeriod.Hour))
            .Series
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(3);
        
        result.Select(r => r.Name).Should().Equal("Investigate", "Linked", "Not Linked");

        result.Should().AllSatisfy(r =>
        {
            r.Periods.Should().AllSatisfy(p =>
            {
                p.Period.Should().BeOnOrAfter(from);
                p.Period.Should().BeOnOrBefore(to);
            });
            r.Periods.Count.Should().Be(24);
        });
    }
    
    [Fact]
    public async Task WhenCalledLastMonth_ReturnExpectedAggregation()
    {
        var result = (await basicSampleDataTestFixture.GetMovementsAggregationService(testOutputHelper)
                .ByCreated(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()))
            .Series
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(3);

        result[1].Name.Should().Be("Linked");
        result[1].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today);
        result[1].Periods.Count.Should().Be(DateTime.Today.DaysSinceMonthAgo() + 1);
        
        result[2].Name.Should().Be("Not Linked");
    }
}
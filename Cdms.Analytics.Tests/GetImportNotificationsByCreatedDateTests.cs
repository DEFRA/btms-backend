using Cdms.Common.Extensions;
using Cdms.Model.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Cdms.Analytics.Tests;

[Collection("Aggregation Test collection")]
public class GetImportNotificationsByCreatedDateTests(
    AggregationTestFixture aggregationTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        var result = (await aggregationTestFixture.LinkingAggregationService
            .ImportNotificationsByCreated(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(),AggregationPeriod.Hour))
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(3);

        result[0].Periods.Count(p => p.Value > 0).Should().BeGreaterThan(1);

        result[0].Name.Should().Be("Cveda Linked");
        result[0].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today.Tomorrow());
        result[0].Periods.Count.Should().Be(48);
        
        result[1].Name.Should().Be("Cveda Not Linked");
        result[1].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today.Tomorrow());
        result[1].Periods.Count.Should().Be(48);
        
        result[2].Name.Should().Be("Cvedp Linked");
        result[2].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today.Tomorrow());
        result[2].Periods.Count.Should().Be(48);
    }
    
    [Fact]
    public async Task WhenCalledLastMonth_ReturnExpectedAggregation()
    {
        var result = (await aggregationTestFixture.LinkingAggregationService
            .ImportNotificationsByCreated(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()))
            .ToList();

        testOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(3);

        result[0].Name.Should().Be("Cveda Linked");
        result[0].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today);
        result[0].Periods.Count.Should().Be(DateTime.Today.DaysSinceMonthAgo() + 1);
        
        result[1].Name.Should().Be("Cveda Not Linked");
        result[1].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today);
        result[1].Periods.Count.Should().Be(DateTime.Today.DaysSinceMonthAgo() + 1);
        
        result[2].Name.Should().Be("Cvedp Linked");
        result[2].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today);
        result[2].Periods.Count.Should().Be(DateTime.Today.DaysSinceMonthAgo() + 1);
    }
}
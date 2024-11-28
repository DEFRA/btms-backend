using Cdms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Cdms.Analytics.Tests;

[Collection("Aggregation Test collection")]
public class GetImportNotificationsByArrivalDateTests(
    AggregationTestFixture aggregationTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalledNextMonth_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        
        var result = (await aggregationTestFixture.LinkingAggregationService
            .GetImportNotificationLinkingByArrival(DateTime.Today, DateTime.Today.MonthLater()))
            .ToList();

        testOutputHelper.WriteLine($"{result.Count} aggregated items found");
        
        //// logger.LogInformation(result.ToJsonString());
            
        result.Count.Should().Be(3);

        result[0].Name.Should().Be("Cveda Linked");
        result[0].Periods[0].Period.Should().BeOnOrAfter(DateTime.Today);
        result[0].Periods.Count.Should().Be(DateTime.Today.DaysUntilMonthLater());
        
        result[1].Name.Should().Be("Cveda Not Linked");
        result[1].Periods[0].Period.Should().BeOnOrAfter(DateTime.Today);
        result[1].Periods.Count.Should().Be(DateTime.Today.DaysUntilMonthLater());

        result[2].Name.Should().Be("Cvedp Linked");
        result[2].Periods[0].Period.Should().BeOnOrAfter(DateTime.Today);
        result[2].Periods.Count.Should().Be(DateTime.Today.DaysUntilMonthLater());
    }
}
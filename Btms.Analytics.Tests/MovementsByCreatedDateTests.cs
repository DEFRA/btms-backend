using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class MovementsByCreatedDateTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{
    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        var result = (await GetMovementsAggregationService()
            .ByCreated(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(), AggregationPeriod.Hour))
            .Series
            .ToList();

        TestOutputHelper.WriteLine(result.ToJsonString());

        result.ShouldBeCorrectBasedOnLinkStatusEnum();

        // result.Count.Should().Be(4);
        // result.Select(r => r.Name).Should().BeEquivalentTo("Investigate", "Linked", "Not Linked", "Error");
        // result[1].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today);
        // result[1].Periods.Count.Should().Be(48);
    }

    [Fact]
    public async Task WhenCalledWithTimePeriodYieldingNoResults_ReturnEmptyAggregation()
    {
        var from = DateTime.MaxValue.AddDays(-1);
        var to = DateTime.MaxValue;

        var result = (await GetMovementsAggregationService()
            .ByCreated(from, to, AggregationPeriod.Hour))
            .Series
            .ToList();

        TestOutputHelper.WriteLine(result.ToJsonString());

        result.ShouldBeCorrectBasedOnLinkStatusEnum(from, to, AggregationPeriod.Hour);
    }

    [Fact]
    public async Task WhenCalledLastMonth_ReturnExpectedAggregation()
    {
        var result = (await GetMovementsAggregationService()
                .ByCreated(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()))
            .Series
            .ToList();

        TestOutputHelper.WriteLine(result.ToJsonString());

        result.ShouldBeCorrectBasedOnLinkStatusEnum(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow());
        // result.Count.Should().Be(4);
        //
        // result[1].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today);
        // result[1].Periods.Count.Should().Be(DateTime.Today.DaysSinceMonthAgo() + 1);
    }
}
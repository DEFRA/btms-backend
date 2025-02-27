using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Btms.Analytics.Tests.Fixtures;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class ImportNotificationsByCreatedDateTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)

{
    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        var result = (await GetImportNotificationsAggregationService()
            .ByCreated(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(), AggregationPeriod.Hour))
            .Series
            .ToList();

        TestOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(8);

        // result[0].Periods.Count(p => p.Value > 0).Should().BeGreaterThan(1);
        result[0].Periods.Sum(p => p.Value)
            .Should().BeGreaterThan(0);

        result[0].Name.Should().Be("CHEDA Linked");
        result[0].Periods[0].Period.Should().BeOnOrBefore(DateTime.Today.Tomorrow());
        result[0].Periods.Count.Should().Be(48);

        result.Sum(r => r.Periods.Sum(p => p.Value))
            .Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task WhenCalledLastMonth_ReturnExpectedAggregation()
    {
        var result = (await GetImportNotificationsAggregationService()
            .ByCreated(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()))
            .Series
            .ToList();

        TestOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(8);

        result[0].Name.Should().Be("CHEDA Linked");

        result.Should().AllSatisfy(r =>
        {
            r.Periods.Should().AllSatisfy(p =>
            {
                p.Period.Should().BeOnOrAfter(DateTime.Today.MonthAgo());
                p.Period.Should().BeOnOrBefore(DateTime.Today.Tomorrow());
            });
            r.Periods.Count.Should().Be(DateTime.Today.DaysSinceMonthAgo() + 1);
        });

        // result.shoul
        result.Sum(r => r.Periods.Sum(p => p.Value))
            .Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task WhenCalledWithTimePeriodYieldingNoResults_ReturnEmptyAggregation()
    {
        var from = DateTime.MaxValue.AddDays(-1);
        var to = DateTime.MaxValue;

        var result = (await GetImportNotificationsAggregationService()
                .ByCreated(from, to, AggregationPeriod.Hour))
            .Series
            .ToList();

        TestOutputHelper.WriteLine(result.ToJsonString());

        result.Count.Should().Be(8);

        result.Select(r => r.Name).Should().Equal(
            "CHEDA Linked", "CHEDA Not Linked", "CHEDD Linked", "CHEDD Not Linked", "CHEDP Linked", "CHEDP Not Linked", "CHEDPP Linked", "CHEDPP Not Linked");

        result.Should().AllSatisfy(r =>
        {
            r.Periods.Should().AllSatisfy(p =>
            {
                p.Period.Should().BeOnOrAfter(from);
                p.Period.Should().BeOnOrBefore(to);
            });
            r.Periods.Count.Should().Be(24);
        });

        result.Sum(r => r.Periods.Sum(p => p.Value))
            .Should().Be(0);
    }
}
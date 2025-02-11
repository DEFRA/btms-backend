using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Model.Ipaffs;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class ImportNotificationsByMaxVersionTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{

    [Fact]
    public async Task WhenCalledLastMonth_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetImportNotificationsAggregationService()
            .ByMaxVersion(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow(), false));

        TestOutputHelper.WriteLine("{0} aggregated items found", result.Values.Count);

        result.Values.Count.Should().Be(1);
    }

    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = await ImportNotificationsAggregationService
            .ByMaxVersion(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(), false);

        TestOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");

        result.Values.Count.Should().Be(1);
    }

    [Fact]
    public async Task WhenCalledWithTimePeriodYieldingNoResults_ReturnEmptyAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = await ImportNotificationsAggregationService
            .ByMaxVersion(DateTime.MaxValue.AddDays(-1), DateTime.MaxValue, false);

        TestOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");

        result.Values.Count.Should().Be(0);
    }

    [Fact]
    public async Task WhenCalledWithChedType_ReturnsResults()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = await ImportNotificationsAggregationService
            .ByMaxVersion(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(), false, chedTypes: [ImportNotificationTypeEnum.Cveda]);

        TestOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");

        result.Values.Count.Should().Be(1);
    }

    [Fact]
    public async Task WhenCalledWithCountry_ReturnsResults()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = await ImportNotificationsAggregationService
            .ByMaxVersion(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(), false, country: "ES");

        TestOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");

        result.Values.Count.Should().Be(1);
    }
}
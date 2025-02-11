using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class ImportNotificationsByArrivalDateTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{

    [Fact]
    public async Task WhenCalledNextMonth_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");

        var result = (await GetImportNotificationsAggregationService()
            .ByArrival(DateTime.Today, DateTime.Today.MonthLater()))
            .Series
            .ToList();

        TestOutputHelper.WriteLine($"{result.Count} aggregated items found");

        result.Count.Should().Be(8);

        result.Should().AllSatisfy(r =>
        {
            r.Periods.Should().AllSatisfy(p =>
            {
                p.Period.Should().BeOnOrAfter(DateTime.Today);
                p.Period.Should().BeOnOrBefore(DateTime.Today.MonthLater());
            });
            r.Periods.Count.Should().Be(DateTime.Today.DaysUntilMonthLater());
        });
    }
}
using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;

namespace Btms.Analytics.Tests;

public class MovementsBySegmentsTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrNoMatchSingleItemWithDecisionScenarioGenerator>(output)
{

    [FailingFact(jiraTicket: "CDMS-205")]
    public async Task WhenCalled_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");

        var result = await Client
            .GetAnalyticsDashboard(["movementsBySegment"],
                dateFrom: DateTime.MinValue, dateTo: DateTime.MaxValue);

        var chart = await result
            .AnalyticsChartAs<SingleSeriesDataset>("movementsBySegment")!;

        chart.Values.Count
            .Should().BeGreaterThan(1);

        chart.Values
            .Single(v => v.Key == "Alvs has match decisions but no Btms links")
            .Value
            .Should().Be(1);

    }
}
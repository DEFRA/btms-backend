using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class MovementsByDecisionsTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{
    
    [Fact(Skip = "Needs revisiting - needs more assertions, perhaps switch to individual scenario test")]
    public async Task WhenCalled_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetMovementsAggregationService()
            .ByDecision(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()))
            .Result;

        TestOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        
        result.Count.Should().BeGreaterThan(1);
        // result.Select(r => r.Key).Order().Should()
        //     .Equal("ALVS Linked : H01", "BTMS Linked : C03", "BTMS Linked : X00", "BTMS Not Linked : X00");
    }
}
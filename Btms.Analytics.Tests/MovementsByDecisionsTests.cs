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
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDecisionsDatasetName)
{
    [Fact]
    public async Task WhenCalled_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = await MovementsAggregationService
            .ByDecision(DateTime.MinValue, DateTime.MaxValue, false)!;

        TestOutputHelper.WriteLine("{0} aggregated items found", result!.Result.Count());

        result.Result.Count.Should().BeGreaterThan(1);
    }
}
using Btms.Common.Extensions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class MovementsDocumentReferencesByMovementTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetMovementsAggregationService()
            .UniqueDocumentReferenceByMovementCount(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()));

        TestOutputHelper.WriteLine("{0} aggregated items found", result.Values.Count);
        
        result.Should().HaveResults();
    }
}
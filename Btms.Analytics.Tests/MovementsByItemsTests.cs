using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class MovementsByItemsTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{
    
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetMovementsAggregationService()
            .ByItemCount(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()))
            .Series
            .ToList();

        TestOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        
        result.Count.Should().Be(3);
        result.Select(r => r.Name).Order().Should().Equal("Investigate", "Linked", "Not Linked");
        
        result.Should().AllSatisfy(r =>
        {
            r.Dimension.Should().Be("Item Count");
            r.Results.Count.Should().NotBe(0);
        });
        
        result.Should().HaveResults();

        result.Should().BeSameLength();
    }
}
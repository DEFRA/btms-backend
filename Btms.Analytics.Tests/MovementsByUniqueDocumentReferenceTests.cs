using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class MovementsByUniqueDocumentReferenceTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{
    
    [Fact(Skip = "Needs revisiting - needs more assertions, perhaps switch to individual scenario test")]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetMovementsAggregationService()
            .ByUniqueDocumentReferenceCount(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()))
            .Series
            .ToList();

        TestOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        
        result.Count().Should().Be(3);
        result.Select(r => r.Name).Order().Should().Equal("Investigate", "Linked", "Not Linked");
        
        result.Should().AllSatisfy(d =>
        {
            d.Dimension.Should().Be("Document Reference Count");
            d.Results.Count().Should().NotBe(0);
            d.Results.Sum(r => ((ByNumericDimensionResult)r).Value).Should().BeGreaterThan(0);
        });
        
        result.Should().HaveResults();
        
        result.Should().BeSameLength();
    }
}
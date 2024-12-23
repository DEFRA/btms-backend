using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
    
namespace Btms.Analytics.Tests;

[Collection(nameof(MultiItemDataTestCollection))]
public class MovementsByUniqueDocumentReferenceTests(
    MultiItemDataTestFixture multiItemDataTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await multiItemDataTestFixture.GetMovementsAggregationService(testOutputHelper)
            .ByUniqueDocumentReferenceCount(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()))
            .Series
            .ToList();

        testOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        
        result.Count().Should().Be(2);
        result.Select(r => r.Name).Order().Should().Equal("Linked", "Not Linked");
        
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
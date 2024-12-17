using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
    
namespace Btms.Analytics.Tests;

[Collection(nameof(MultiItemDataTestCollection))]
public class MovementsByItemsTests(
    MultiItemDataTestFixture multiItemDataTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await multiItemDataTestFixture.GetMovementsAggregationService(testOutputHelper)
            .ByItemCount(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()))
            .Series
            .ToList();

        testOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        
        result.Count.Should().Be(2);
        result.Select(r => r.Name).Order().Should().Equal("Linked", "Not Linked");
        
        result.Should().AllSatisfy(r =>
        {
            r.Dimension.Should().Be("Item Count");
            r.Results.Count.Should().NotBe(0);
        });
        
        result.Should().HaveResults();

        result.Should().BeSameLength();
    }
}
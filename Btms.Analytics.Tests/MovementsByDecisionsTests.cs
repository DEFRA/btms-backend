using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
    
namespace Btms.Analytics.Tests;

[Collection(nameof(MultiItemDataTestCollection))]
public class MovementsByDecisionsTests(
    MultiItemDataTestFixture multiItemDataTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalled_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await multiItemDataTestFixture.GetMovementsAggregationService(testOutputHelper)
            .ByDecision(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()))
            .Series
            .ToList();

        testOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        
        result.Count.Should().Be(2);
        
        result.Should().BeSameLength();
    }
}
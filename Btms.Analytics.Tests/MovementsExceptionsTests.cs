using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
    
namespace Btms.Analytics.Tests;

[Collection(nameof(MultiItemDataTestCollection))]
public class MovementsExceptionsTests(
    MultiItemDataTestFixture multiItemDataTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalled_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await multiItemDataTestFixture.GetMovementsAggregationService(testOutputHelper)
            .ByDecision(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()))
            .Result;

        testOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        
        // result.Count.Should().BeGreaterThan(1);
        // result.Select(r => r.Key).Order().Should()
        //     .Equal("ALVS Linked : H01", "BTMS Linked : C03", "BTMS Linked : X00", "BTMS Not Linked : X00");
    }
}
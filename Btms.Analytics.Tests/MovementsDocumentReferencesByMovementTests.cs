using Btms.Common.Extensions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
    
namespace Btms.Analytics.Tests;

[Collection(nameof(MultiItemDataTestCollection))]
public class MovementsDocumentReferencesByMovementTests(
    MultiItemDataTestFixture multiItemDataTestFixture,
    ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await multiItemDataTestFixture.MovementsAggregationService
            .UniqueDocumentReferenceByMovementCount(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()));

        testOutputHelper.WriteLine("{0} aggregated items found", result.Values.Count);
        
        result.Should().HaveResults();
    }
}
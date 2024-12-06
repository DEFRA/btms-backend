using Btms.Analytics.Extensions;
using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Btms.Analytics.Tests.Helpers;
using Btms.Analytics.Tests.Fixtures;
    
namespace Btms.Analytics.Tests;

[Collection(nameof(MultiItemDataTestCollection))]
public class ImportNotificationsByCommoditiesTests(
    MultiItemDataTestFixture multiItemDataTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await multiItemDataTestFixture.ImportNotificationsAggregationService
            .ByCommodityCount(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()))
            .ToList();;

        testOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        
        result.Count().Should().Be(8);
        result.Select(r => r.Name).Order().Should().Equal(AnalyticsHelpers.GetImportNotificationSegments().Order());
        
        result.Should().AllSatisfy(r =>
        {
            r.Dimension.Should().Be("ItemCount");
            r.Results.Count().Should().NotBe(0);
        });
        
        result.Should().HaveResults();
        
        result.Should().BeSameLength();
    }
}
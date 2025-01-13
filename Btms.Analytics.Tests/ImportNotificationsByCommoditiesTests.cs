using Btms.Analytics.Extensions;
using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Btms.Analytics.Tests.Helpers;
using Btms.Analytics.Tests.Fixtures;
using Btms.Model.Extensions;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class ImportNotificationsByCommoditiesTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{
    
    [Fact]
    public async Task WhenCalledLastWeek_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetImportNotificationsAggregationService()
            .ByCommodityCount(DateTime.Today.WeekAgo(), DateTime.Today.Tomorrow()))
            .Series
            .ToList();

        TestOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        TestOutputHelper.WriteLine(result.ToJsonString());
        
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
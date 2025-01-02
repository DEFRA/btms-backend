using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Model.Ipaffs;

namespace Btms.Analytics.Tests;

[Collection(nameof(BasicSampleDataTestCollection))]
public class ImportNotificationsByMaxVersionTests(
    BasicSampleDataTestFixture basicSampleDataTestFixture,
    ITestOutputHelper testOutputHelper)
{
    
    [Fact]
    public async Task WhenCalledLastMonth_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await basicSampleDataTestFixture.GetImportNotificationsAggregationService(testOutputHelper)
            .ByMaxVersion(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()));

        testOutputHelper.WriteLine("{0} aggregated items found", result.Values.Count);
        
        result.Values.Count.Should().Be(1);
    }
    
    [Fact]
    public async Task WhenCalledLast48Hours_ReturnExpectedAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await basicSampleDataTestFixture.GetImportNotificationsAggregationService(testOutputHelper)
            .ByMaxVersion(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour()));

        testOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(1);

    }
    
    [Fact]
    public async Task WhenCalledWithTimePeriodYieldingNoResults_ReturnEmptyAggregation()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await basicSampleDataTestFixture.GetImportNotificationsAggregationService(testOutputHelper)
            .ByMaxVersion(DateTime.MaxValue.AddDays(-1), DateTime.MaxValue));

        testOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(0);
    }
    
    [Fact]
    public async Task WhenCalledWithChedType_ReturnsResults()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await basicSampleDataTestFixture.GetImportNotificationsAggregationService(testOutputHelper)
            .ByMaxVersion(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(), chedTypes: [ImportNotificationTypeEnum.Cveda]));

        testOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(1);
    }
    
    [Fact]
    public async Task WhenCalledWithCountry_ReturnsResults()
    {
        testOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await basicSampleDataTestFixture.GetImportNotificationsAggregationService(testOutputHelper)
            .ByMaxVersion(DateTime.Now.NextHour().AddDays(-2), DateTime.Now.NextHour(), country: "AL"));

        testOutputHelper.WriteLine($"{result.Values.Count} aggregated items found");
        
        result.Values.Count.Should().Be(1);
    }
}
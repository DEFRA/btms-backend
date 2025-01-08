using Btms.Model.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Analytics.Tests;

public class MovementsByMaxVersionTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrNoMatchSingleItemWithDecisionScenarioGenerator>(output)
{
    
    [Fact]
    public async Task WhenCalled_ReturnsResults()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");

        var result = await Client
            .GetAnalyticsDashboard(["movementsByMaxEntryVersion"],
                dateFrom:DateTime.Today.AddDays(-1),
                dateTo:DateTime.Today.AddDays(1));
        
        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());
        
        var charts = await result.ToJsonDictionary();
        
        TestOutputHelper.WriteLine($"movementsByMaxEntryVersion keys : {charts["movementsByMaxEntryVersion"].GetKeys()}");
        TestOutputHelper.WriteLine($"result keys : {charts["movementsByMaxEntryVersion"]["values"]!.GetKeys()}");

        charts["movementsByMaxEntryVersion"]["values"]!
            .GetKeys()
            .Length.Should().Be(1);
        
        var val = charts["movementsByMaxEntryVersion"]["values"]!["1"]!
            .GetValue<int>()
            .Should()
            .Be(1);
    }
    
    [Fact]
    public async Task WhenCalledWithChedType_ReturnsResults()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");

        var result = await Client
            .GetAnalyticsDashboard(["movementsByMaxEntryVersion"],
                dateFrom:DateTime.Today.AddDays(-1),
                dateTo:DateTime.Today.AddDays(1),
                chedTypes: [ImportNotificationTypeEnum.Cvedp]);
        
        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());
        
        var charts = await result.ToJsonDictionary();
        
        TestOutputHelper.WriteLine($"movementsByMaxEntryVersion keys : {charts["movementsByMaxEntryVersion"].GetKeys()}");
        TestOutputHelper.WriteLine($"result keys : {charts["movementsByMaxEntryVersion"]["values"]!.GetKeys()}");

        charts["movementsByMaxEntryVersion"]["values"]!
            .GetKeys()
            .Length.Should().Be(1);
        
        var val = charts["movementsByMaxEntryVersion"]["values"]!["1"]!
            .GetValue<int>()
            .Should()
            .Be(1);
    }
    
    [Fact]
    public async Task WhenCalledWithWrongChedType_ReturnsResults()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");

        var result = await Client
            .GetAnalyticsDashboard(["movementsByMaxEntryVersion"],
                dateFrom:DateTime.Today.AddDays(-1),
                dateTo:DateTime.Today.AddDays(1),
                chedTypes: [ImportNotificationTypeEnum.Cveda]);
        
        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());
        
        var charts = await result.ToJsonDictionary();
        
        TestOutputHelper.WriteLine($"movementsByMaxEntryVersion keys : {charts["movementsByMaxEntryVersion"].GetKeys()}");
        TestOutputHelper.WriteLine($"result keys : {charts["movementsByMaxEntryVersion"]["values"]!.GetKeys()}");

        charts["movementsByMaxEntryVersion"]["values"]!
            .GetKeys()
            .Length.Should().Be(0);
    }
    
    [Fact]
    public async Task WhenCalledWithCountry_ReturnsResults()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");

        var result = await Client
            .GetAnalyticsDashboard(["movementsByMaxEntryVersion"],
                dateFrom:DateTime.Today.AddDays(-1),
                dateTo:DateTime.Today.AddDays(1),
                country:"FR");
        
        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());
        
        var charts = await result.ToJsonDictionary();
        
        TestOutputHelper.WriteLine($"movementsByMaxEntryVersion keys : {charts["movementsByMaxEntryVersion"].GetKeys()}");
        TestOutputHelper.WriteLine($"result keys : {charts["movementsByMaxEntryVersion"]["values"]!.GetKeys()}");

        charts["movementsByMaxEntryVersion"]["values"]!
            .GetKeys()
            .Length.Should().Be(1);
        
        var val = charts["movementsByMaxEntryVersion"]["values"]!["1"]!
            .GetValue<int>()
            .Should()
            .Be(1);
    }
    
    
    [Fact]
    public async Task WhenCalledWithWrongCountry_ReturnsResults()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");

        var result = await Client   
            .GetAnalyticsDashboard(["movementsByMaxEntryVersion"],
                dateFrom:DateTime.Today.AddDays(-1),
                dateTo:DateTime.Today.AddDays(1),
                country:"ES");
        
        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());
        
        var charts = await result.ToJsonDictionary();
        
        TestOutputHelper.WriteLine($"movementsByMaxEntryVersion keys : {charts["movementsByMaxEntryVersion"].GetKeys()}");
        TestOutputHelper.WriteLine($"result keys : {charts["movementsByMaxEntryVersion"]["values"]!.GetKeys()}");

        charts["movementsByMaxEntryVersion"]["values"]!
            .GetKeys()
            .Length.Should().Be(0);
    }
}
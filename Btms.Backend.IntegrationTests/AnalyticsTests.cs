using Btms.Backend.IntegrationTests.Extensions;
using Btms.Backend.IntegrationTests.Helpers;
using FluentAssertions;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class AnalyticsTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<ApplicationFactory>
{
    
    [Fact]
    public async Task GetIndividualMultiSeriesDatetimeChart()
    {
        //Act
        var response = await Client.GetAnalyticsDashboard(["importNotificationLinkingByCreated"]);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(response.StatusCode.ToString());
        
        var responseDictionary = await response.ToJsonDictionary();
        
        responseDictionary.Count.Should().Be(1);
        
        responseDictionary.Keys.Should().Equal("importNotificationLinkingByCreated");

    }
    
    [Fact]
    public async Task GetIndividualMultiSeriesChart()
    {
        //Act
        var response = await Client.GetAnalyticsDashboard(["lastMonthMovementsByItemCount"]);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(response.StatusCode.ToString());
        
        var responseDictionary = await response.ToJsonDictionary();
        
        responseDictionary.Count.Should().Be(1);
        
        responseDictionary.Keys.Should().Equal("lastMonthMovementsByItemCount");

    }
    
    [Fact]
    public async Task GetIndividualSingleSeriesChart()
    {
        //Act
        var response = await Client.GetAnalyticsDashboard(["last7DaysImportNotificationsLinkingStatus"]);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(response.StatusCode.ToString());
        
        var responseDictionary = await response.ToJsonDictionary();
        
        responseDictionary.Count.Should().Be(1);
        
        responseDictionary.Keys.Should().Equal("last7DaysImportNotificationsLinkingStatus");

    }

    [Fact]
    public async Task GetAllCharts()
    {
        //Act
        var response = await Client.GetAnalyticsDashboard();
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(response.StatusCode.ToString());
        
        var responseDictionary = await response.ToJsonDictionary();
        
        responseDictionary.Keys.Take(2).Should().Equal(
            "importNotificationLinkingByCreated",
            "importNotificationLinkingByArrival");
        
        responseDictionary["importNotificationLinkingByCreated"].Should().NotBeNull();
    }
}
using System.Diagnostics.CodeAnalysis;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.SyncJob;
using Btms.Backend.IntegrationTests.JsonApiClient;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Btms.Backend.IntegrationTests.Extensions;
using Btms.Backend.IntegrationTests.Helpers;
using Json.More;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class AnalyticsTests(IntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<IntegrationTestsApplicationFactory>
{
    
    // private static void ShouldNotBeNull<T>([DoesNotReturnIf(true), NotNull] T? value)
    // {
    //     // throw if null
    //     value.Should().NotBeNull();
    // }
    
    [Fact]
    public async Task GetIndividualMultiSeriesDatetimeChart()
    {
        //Act
        var response = await Client.GetAsync(
            "/analytics/dashboard?chartsToRender=importNotificationLinkingByCreated");
        
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
        var response = await Client.GetAsync(
            "/analytics/dashboard?chartsToRender=lastMonthMovementsByItemCount");
        
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
        var response = await Client.GetAsync(
            "/analytics/dashboard?chartsToRender=last7DaysImportNotificationsLinkingStatus");
        
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
        var response = await Client.GetAsync("/analytics/dashboard");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(response.StatusCode.ToString());
        
        var responseDictionary = await response.ToJsonDictionary();
        
        responseDictionary.Keys.Take(2).Should().Equal(
            "importNotificationLinkingByCreated",
            "importNotificationLinkingByArrival");
        
        responseDictionary["importNotificationLinkingByCreated"].Should().NotBeNull();
    }
}
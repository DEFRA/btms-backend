using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using Btms.Backend.IntegrationTests.JsonApiClient;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class LinkingTests(IntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<IntegrationTestsApplicationFactory>
{
    [Fact]
    public async Task SyncClearanceRequests_WithNoReferencedNotifications_ShouldNotLink()
    {
        // Arrange
        await IntegrationTestsApplicationFactory.ClearDb(Client);

        // Act
        await MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });

        // Assert
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/movements");
        jsonClientResponse.Data
            .Where(x => x.Relationships is not null)
            .SelectMany(x => x.Relationships!)
            .Any(x => x.Value is { Links: not null })
            .Should().Be(false);
    }

    [Fact]
    public async Task SyncClearanceRequests_WithReferencedNotifications_ShouldLink()
    {
        // Arrange
        await IntegrationTestsApplicationFactory.ClearDb(Client);

        // Act
        await MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });
        await MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });

        // Assert
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/movements");
        jsonClientResponse.Data
            .Where(x => x.Relationships is not null)
            .SelectMany(x => x.Relationships!)
            .Any(x => x.Value is { Links: not null })
            .Should().Be(true);
    }
        
    [Fact]
    public async Task SyncNotifications_WithNoReferencedMovements_ShouldNotLink()
    {
        // Arrange
        await IntegrationTestsApplicationFactory.ClearDb(Client);
            
        // Act
        await MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });

        // Assert
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/import-notifications");
        jsonClientResponse.Data
            .Where(x => x.Relationships is not null)
            .SelectMany(x => x.Relationships!)
            .Any(x => x.Value is { Links: not null })
            .Should().Be(false);
    }
        
    [Fact]
    public async Task SyncNotifications_WithReferencedMovements_ShouldLink()
    {
        // Arrange
        await IntegrationTestsApplicationFactory.ClearDb(Client);
            
        // Act
        await MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });
        await MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });

        // Assert
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/import-notifications");
        jsonClientResponse.Data
            .Where(x => x.Relationships is not null)
            .SelectMany(x => x.Relationships!)
            .Any(x => x.Value is { Links: not null })
            .Should().Be(true);
    }
}
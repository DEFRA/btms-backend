using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class LinkingTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<ApplicationFactory>
{
    [Fact]
    public async Task SyncClearanceRequests_WithNoReferencedNotifications_ShouldNotLink()
    {
        // Arrange
        await Client.ClearDb();

        // Act
        await Client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
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
        await ClearDb();

        // Act
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });
        await Client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
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
        await ClearDb();

        // Act
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
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
        await ClearDb();

        // Act
        await Client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        // Assert
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/import-notifications");
        jsonClientResponse.Data
            .Where(x => x.Relationships is not null)
            .SelectMany(x => x.Relationships!)
            .Any(x => x.Value is { Links: not null })
            .Should().Be(true);
    }

    [Fact]
    public async Task ImportNotification_WhenClearanceRequest_ResourceUpdated_UpdatedFieldOnResource_ShouldNotChange()
    {
        await ClearDb();

        // Import notifications
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        var document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");
        var updated = DateTime.Parse((document.Data.Attributes?["updated"]!).ToString()!);
        var updatedEntity = DateTime.Parse((document.Data.Attributes?["updatedEntity"]!).ToString()!);

        // Import clearance requests and initial linking will take place
        await Client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");
        var updatedPostLink = DateTime.Parse((document.Data.Attributes?["updated"]!).ToString()!);
        var updatedEntityPostLink = DateTime.Parse((document.Data.Attributes?["updatedEntity"]!).ToString()!);

        updated.Should().Be(updatedPostLink);
        updatedEntity.Should().BeBefore(updatedEntityPostLink);

        // Import new clearance version, link will already exist, but UpdateEntity will still change
        await Client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "Linking"
        });

        document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");
        var updatedPostMovementUpdate = DateTime.Parse((document.Data.Attributes?["updated"]!).ToString()!);
        var updatedEntityPostMovementUpdate = DateTime.Parse((document.Data.Attributes?["updatedEntity"]!).ToString()!);

        updatedPostLink.Should().Be(updatedPostMovementUpdate);
        updatedEntityPostLink.Should().BeBefore(updatedEntityPostMovementUpdate);
    }

    [Fact]
    public async Task ImportNotification_WhenGmr_ResourceUpdated_UpdatedFieldOnResource_ShouldNotChange()
    {
        await ClearDb();

        // Import notifications
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        var document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");
        var updated = DateTime.Parse((document.Data.Attributes?["updated"]!).ToString()!);
        var updatedEntity = DateTime.Parse((document.Data.Attributes?["updatedEntity"]!).ToString()!);

        // Import GMRs and initial linking will take place
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");
        var updatedPostLink = DateTime.Parse((document.Data.Attributes?["updated"]!).ToString()!);
        var updatedEntityPostLink = DateTime.Parse((document.Data.Attributes?["updatedEntity"]!).ToString()!);

        updated.Should().Be(updatedPostLink);
        updatedEntity.Should().BeBefore(updatedEntityPostLink);

        // Import new GMR, link will already exist, but UpdateEntity will still change
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "Linking"
        });

        document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");
        var updatedPostGmrUpdate = DateTime.Parse((document.Data.Attributes?["updated"]!).ToString()!);
        var updatedEntityPostGmrUpdate = DateTime.Parse((document.Data.Attributes?["updatedEntity"]!).ToString()!);

        updatedPostLink.Should().Be(updatedPostGmrUpdate);
        updatedEntityPostLink.Should().BeBefore(updatedEntityPostGmrUpdate);
    }

    [Fact]
    public async Task SyncGmrs_WithReferenceNotifications_ShouldLink()
    {
        await Client.ClearDb();
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        var document = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        document.Data.Id.Should().Be("GMRAPOQSPDUG");

        document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");

        document.Data.Id.Should().Be("CHEDA.GB.2024.1041389");
        document.Data.Relationships?["gmrs"]!.Data.ManyValue.Should().ContainEquivalentOf(new { Id = "GMRAPOQSPDUG" });

        // Import new version, link remains and no additional relationships should be added
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "Linking"
        });

        document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");

        document.Data.Id.Should().Be("CHEDA.GB.2024.1041389");
        document.Data.Relationships?["gmrs"]!.Data.ManyValue.Should().ContainSingle().And
            .ContainEquivalentOf(new { Id = "GMRAPOQSPDUG" });

        document = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");
        document.Data.Relationships?["import-notifications"]!.Data.ManyValue.Should().HaveCount(2).And
            .ContainEquivalentOf(new { Id = "CHEDA.GB.2024.1041389" }).And
            .ContainEquivalentOf(new { Id = "CHEDD.GB.2024.1004768" });
    }

    [Fact]
    public async Task SyncNotifications_WithReferenceGmrs_ShouldLink()
    {
        await Client.ClearDb();
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        var document = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        document.Data.Id.Should().Be("GMRAPOQSPDUG");

        document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");

        document.Data.Id.Should().Be("CHEDA.GB.2024.1041389");
        document.Data.Relationships?["gmrs"]!.Data.ManyValue.Should().ContainEquivalentOf(new { Id = "GMRAPOQSPDUG" });

        // Import new version, link remains and no additional relationships should be added
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "Linking"
        });

        document = Client.AsJsonApiClient().GetById("CHEDA.GB.2024.1041389", "api/import-notifications");

        document.Data.Id.Should().Be("CHEDA.GB.2024.1041389");
        document.Data.Relationships?["gmrs"]!.Data.ManyValue.Should().ContainSingle().And
            .ContainEquivalentOf(new { Id = "GMRAPOQSPDUG" });

        document = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");
        document.Data.Relationships?["import-notifications"]!.Data.ManyValue.Should().HaveCount(2).And
            .ContainEquivalentOf(new { Id = "CHEDA.GB.2024.1041389" }).And
            .ContainEquivalentOf(new { Id = "CHEDD.GB.2024.1004768" });
    }
}
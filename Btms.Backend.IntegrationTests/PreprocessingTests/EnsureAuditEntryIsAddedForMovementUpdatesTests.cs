using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Types.Alvs;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestDataGenerator.Scenarios.SpecificFiles;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.PreprocessingTests;

[Trait("Category", "Integration")]
public class EnsureAuditEntryIsAddedForMovementUpdatesTests(InMemoryScenarioApplicationFactory<NoAuditLogForMovementUpdate> factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "DecisionTests"), IClassFixture<InMemoryScenarioApplicationFactory<NoAuditLogForMovementUpdate>>
{
    
    // [Fact(Skip = "We're ending up with 2 items on the clearance request here.")]
    [Fact]
    public void ShouldNotCreateDuplicateItems()
    {
        // Arrange
        var loadedData = factory.LoadedData;
        
        var movementMessage = (AlvsClearanceRequest)loadedData.First(d =>
                d is { message: AlvsClearanceRequest })
            .message;
        
        // Act
        var jsonClientNotificationsResponse = Client.AsJsonApiClient().Get("api/import-notifications");
        jsonClientNotificationsResponse.Data.Count.Should().Be(3);
        
        // Act
        var jsonClientResponse = Client.AsJsonApiClient().GetById(movementMessage!.Header!.EntryReference!, "api/movements");
        
        // Assert
        jsonClientResponse.Should().NotBeNull();
        jsonClientResponse.Data.Relationships!.Count.Should().Be(1);
        
        var movement = jsonClientResponse.GetResourceObject<Movement>();

        movement.Items.Count.Should().Be(2);
        movement.Items.First().Documents!.First().DocumentReference.Should().Be("GBCHD2024.001239999999");
        movement.AuditEntries
            .Count(a => a is { CreatedBy: "Cds", Status: "Updated" })
            .Should().Be(1);
        
        // TODO : for some reason whilst jsonClientResponse contains the notification relationship, movement doesn't!
        // movement.Relationships.Notifications.Data.Count.Should().Be(1);
    }
}
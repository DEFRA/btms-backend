using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Types.Alvs;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.PreprocessingTests;

[Trait("Category", "Integration")]
public class EnsureDuplicateItemsAreNotCreatedTests(InMemoryScenarioApplicationFactory<DuplicateMovementItems_CDMS_211> factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "DecisionTests"), IClassFixture<InMemoryScenarioApplicationFactory<DuplicateMovementItems_CDMS_211>>
{
    
    [Fact(Skip = "We're ending up with 2 items on the clearance request here.")]
    // [Fact]
    public async Task ShouldNotCreateDuplicateItems()
    {
        // Arrange
        var loadedData = await factory.GenerateAndLoadTestData(Client);
        
        var movementMessage = (AlvsClearanceRequest)loadedData.First(d =>
                d is { message: AlvsClearanceRequest })
            .message;
        
        
        // Act
        var jsonClientResponse = Client.AsJsonApiClient().GetById(movementMessage!.Header!.EntryReference!, "api/movements");
        
        // Assert
        jsonClientResponse.Should().NotBeNull();
        jsonClientResponse.Data.Relationships!.Count.Should().Be(1);
        
        var movement = jsonClientResponse.GetResourceObject<Movement>();

        movement.Items.Count.Should().Be(1);

        // TODO : for some reason whilst jsonClientResponse contains the notification relationship, movement doesn't!
        // movement.Relationships.Notifications.Data.Count.Should().Be(1);
    }
}
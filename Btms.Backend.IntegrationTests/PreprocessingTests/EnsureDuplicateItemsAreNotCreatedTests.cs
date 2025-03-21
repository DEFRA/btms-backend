using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Types.Alvs;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.PreprocessingTests;

[Trait("Category", "Integration")]
public class EnsureDuplicateItemsAreNotCreatedTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<DuplicateMovementItemsCdms211>(output)
{
    [Fact]
    public void ShouldNotCreateDuplicateItems()
    {
        // Arrange
        var loadedData = LoadedData;

        var movementMessage = (AlvsClearanceRequest)loadedData.First(d =>
                d is { Message: AlvsClearanceRequest })
            .Message;

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
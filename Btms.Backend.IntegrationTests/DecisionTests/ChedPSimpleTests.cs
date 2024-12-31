using Btms.Model;
using FluentAssertions;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Types.Alvs;
using TestDataGenerator.Scenarios.ChedP;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class ChedPSimpleTests(InMemoryScenarioApplicationFactory<SimpleMatchScenarioGenerator> factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "DecisionTests"), IClassFixture<InMemoryScenarioApplicationFactory<SimpleMatchScenarioGenerator>>
{
    
    [Fact]
    public void SimpleChedPScenario_ShouldBeLinkedAndMatchDecision()
    {
        // Arrange
        var loadedData = factory.LoadedData;
        
        var chedPMovement = (AlvsClearanceRequest)loadedData.Single(d =>
                d is { message: AlvsClearanceRequest })
            .message;
        
        var chedPNotification = (Types.Ipaffs.ImportNotification)loadedData.Single(d =>
                d is { message: Types.Ipaffs.ImportNotification })
            .message;

        // Act
        var jsonClientResponse = Client.AsJsonApiClient().GetById(chedPMovement!.Header!.EntryReference!, "api/movements");
        
        // Assert
        jsonClientResponse.Should().NotBeNull();
        jsonClientResponse.Data.Relationships!.Count.Should().Be(1);
        
        var movement = jsonClientResponse.GetResourceObject<Movement>();
        movement.Decisions.Count.Should().Be(2);
        movement.AlvsDecisionStatus.Decisions.Count.Should().Be(1);
        
        movement.AlvsDecisionStatus.Decisions
            .First()
            .Context.DecisionMatched
            .Should().BeTrue();

        movement.AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version))
            .Should()
            .Equal([
                ("Cds", "Created", 1),
                ("Btms", "Decision", 1),
                ("Btms", "Linked", null),
                ("Btms", "Decision", 2),
                ("Alvs", "Decision", 1)
            ]);

        var decisionWithLinkAndContext = movement.AuditEntries
            .Where(a => a.CreatedBy == "Btms" && a.Status == "Decision")
            .MaxBy(a => a.Version)!;

        decisionWithLinkAndContext.Context!.ImportNotifications
            .Should().NotBeNull();
        
        decisionWithLinkAndContext.Context!.ImportNotifications!
            .Select(n => (n.Id, n.Version))
            .Should().Equal([
                ( chedPNotification.ReferenceNumber!, 1 )
            ]);

        // TODO : for some reason whilst jsonClientResponse contains the notification relationship, movement doesn't!
        // movement.Relationships.Notifications.Data.Count.Should().Be(1);
    }
}
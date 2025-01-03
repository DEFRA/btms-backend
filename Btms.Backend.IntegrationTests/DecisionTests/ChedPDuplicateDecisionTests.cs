using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class ChedPDuplicateDecisionTests(ITestOutputHelper output)
    : BaseTest<SimpleMatchScenarioGenerator>(output)
{            
    // [Fact(Skip = "We currently import the duplicate alvs decision & store it on the movement")]
    [Fact]
    public void SimpleChedPScenario_ShouldBeLinkedAndMatchDecision()
    {
        // Arrange
        var loadedData = LoadedData;
        
        var chedPMovement = (AlvsClearanceRequest)loadedData.Single(d =>
                d is { Message: AlvsClearanceRequest })
            .Message;
        
        var chedPNotification = (ImportNotification)loadedData.Single(d =>
                d is { Message: ImportNotification })
            .Message;

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
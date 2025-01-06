using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
// public class ChedPUpdatedNotificationTests(InMemoryScenarioApplicationFactory<MultiStepScenarioGenerator> factory, ITestOutputHelper testOutputHelper)
//     : BaseApiTests(factory, testOutputHelper, "DecisionTests"), IClassFixture<InMemoryScenarioApplicationFactory<MultiStepScenarioGenerator>>

public class ChedPUpdatedNotificationTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<MultiStepScenarioGenerator>(output)
{
    
    [Fact(Skip = "We're not currently re-linking when a new version of notification arrives it seems")]
    // [Fact]
    public void MultipleNotificationVersionScenario_ShouldBeLinkedAndMatchDecision()
    {
        // Arrange
        var loadedData = LoadedData;
        
        var chedPMovement = (AlvsClearanceRequest)loadedData.Single(d =>
                d is { Message: AlvsClearanceRequest })
            .Message;
        
        // The scenario has multiple versions of the same notification so we just want one of them.
        var chedPNotification = (ImportNotification)LoadedData.Single(d =>
                d is { Message: ImportNotification })
            .Message;

        // Act
        var jsonClientResponse = Client.AsJsonApiClient().GetById(chedPMovement!.Header!.EntryReference!, "api/movements");
        
        // Assert
        jsonClientResponse.Should().NotBeNull();
        jsonClientResponse.Data.Relationships!.Count.Should().Be(1);
        
        var movement = jsonClientResponse.GetResourceObject<Movement>();
        
        // TODO : We should make 3 decisions:
        // The initial movement creation
        // v1 of the notification
        // v2 of the notification

        movement.Decisions.Count.Should().Be(3);
        movement.AlvsDecisionStatus.Decisions.Count.Should().Be(1);
        
        movement.AlvsDecisionStatus.Decisions
            .First()
            .Context.DecisionMatched
            .Should().BeTrue();

        movement.AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version, a.Context?.ImportNotifications?.FirstOrDefault()?.Version))
            .Should()
            .Equal([
                ("Cds", "Created", 1, null),
                ("Btms", "Decision", 1, null),
                ("Btms", "Linked", null, null), //TODO : can we get context in here including the notification info
                ("Btms", "Decision", 2, 1),
                ("Alvs", "Decision", 1, 0), //TODO : we should be able to use the IBM provided file to get some context
                ("Btms", "Decision", 2, 2)
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
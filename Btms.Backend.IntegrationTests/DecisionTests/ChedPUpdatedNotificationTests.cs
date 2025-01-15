using Btms.Model.Auditing;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class ChedPUpdatedNotificationTests
    : ScenarioGeneratorBaseTest<MultiStepScenarioGenerator>
{
    // public required AlvsClearanceRequest ChedPMovement;
    public required ImportNotification ChedPNotification;

    public ChedPUpdatedNotificationTests(ITestOutputHelper output): base(output)
    {
        // ChedPMovement = (AlvsClearanceRequest)LoadedData.Single(d =>
        //         d is { Message: AlvsClearanceRequest })
        //     .Message;
        
        // The scenario has multiple versions of the same notification so we just want one of them.
        ChedPNotification = (ImportNotification)LoadedData.First(d =>
                d is { Message: ImportNotification })
            .Message;
    }

    // This scenario has an update adding a commodity that gets 
    // processed but doesn't cause a new decision
    // [FailingFact(jiraTicket:"CDMS-234"), Trait("JiraTicket", "CDMS-234")]
    [Fact]
    public void ShouldHaveCorrectAuditEntries()
    {
        var movement = Client
            .GetSingleMovement();
            
        movement.AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version, a.Context?.ImportNotifications?.FirstOrDefault()?.Version))
            .Should()
            .Equal([
                (CreatedBySystem.Cds, "Created", 1, null),
                (CreatedBySystem.Btms, "Decision", 1, null),
                (CreatedBySystem.Btms, "Linked", null, null), //TODO : can we get context in here including the notification info
                (CreatedBySystem.Btms, "Decision", 2, 1),
                (CreatedBySystem.Btms, "Decision", 3, 2),
                (CreatedBySystem.Alvs, "Decision", 1, null), //TODO : we should be able to use the IBM provided file to get some context

            ]);
    }

    [FailingFact(jiraTicket:"CDMS-234"), Trait("JiraTicket", "CDMS-234")]
    // [Fact]
    public void ShouldHave3BtmsDecisions()
    {
        var movement = Client
            .GetSingleMovement();
        
        // TODO : We should make 3 decisions:
        // The initial movement creation
        // v1 of the notification
        // v2 of the notification

        movement.Decisions.Count.Should().Be(3);
    }
    

    [Fact]
    public void ShouldHave1AlvsDecision()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions.Count
            .Should().Be(1);
    }

    [Fact]
    public void AlvsDecisionShouldBeMatched()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .Single()
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }

    [Fact]
    public void AlvsDecisionShouldBePaired()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.Paired
            .Should().BeTrue();
    }

    [Fact]
    public void LastBtmsDecisionShouldHaveCorrectAuditEntry()
    {
        var movement = Client
            .GetSingleMovement();

        var decisionWithLinkAndContext = movement.AuditEntries
            .Where(a => a is { CreatedBy: CreatedBySystem.Btms, Status: "Decision" })
            .MaxBy(a => a.Version)!;

        decisionWithLinkAndContext.Context!.ImportNotifications
            .Should().NotBeNull();

        decisionWithLinkAndContext.Context!.ImportNotifications!
            .Select(n => (n.Id, n.Version))
            .Should().Equal([
                (ChedPNotification.ReferenceNumber!, 2)
            ]);
    }
}
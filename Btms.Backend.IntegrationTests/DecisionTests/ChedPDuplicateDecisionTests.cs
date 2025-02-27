using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Model.Cds;
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
    : ScenarioGeneratorBaseTest<SimpleMatchCrFirstScenarioGenerator>(output)
{
    // [Fact(Skip = "We currently import the duplicate alvs decision & store it on the movement")]
    [Fact]
    public void SimpleChedPScenario_ShouldBeLinkedAndMatchDecision()
    {
        // Arrange

        var chedPMovement = (AlvsClearanceRequest)LoadedData.Single(d =>
                d is { Message: AlvsClearanceRequest })
            .Message;

        var chedPNotification = (ImportNotification)LoadedData.Single(d =>
                d is { Message: ImportNotification })
            .Message;

        AddAdditionalContextToAssertFailures(() =>
        {
            var jsonClientResponse = Client.AsJsonApiClient()
                .GetById(chedPMovement!.Header!.EntryReference!, "api/movements");

            // Assert
            jsonClientResponse.Should().NotBeNull();

            var movement = jsonClientResponse.GetResourceObject<Movement>();

            // It appears we can get 1, or 2 decisions, depending on how this test
            // runs - assume because of timing
            movement.Decisions.Count.Should().BeGreaterThan(0);
            movement.AlvsDecisionStatus.Decisions.Count.Should().Be(1);

            movement.AlvsDecisionStatus.Decisions
                .First()
                .Context.DecisionComparison!.DecisionMatched
                .Should().BeTrue();

            var decisionWithLinkAndContext = movement.AuditEntries
                .Where(a => a.CreatedBy == CreatedBySystem.Btms && a.Status == "Decision")
                .MaxBy(a => a.Version)!;

            decisionWithLinkAndContext.Context.As<DecisionContext>()!.ImportNotifications
                .Should().NotBeNull();

            decisionWithLinkAndContext.Context.As<DecisionContext>()!.ImportNotifications!
                .Select(n => (n.Id, n.Version))
                .Should().Equal([
                    (chedPNotification.ReferenceNumber!, 1)
                ]);
        });

    }
}
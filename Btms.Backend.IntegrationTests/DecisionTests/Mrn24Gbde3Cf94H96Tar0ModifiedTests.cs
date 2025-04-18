using System.Net;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Model.Cds;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;
using ImportNotificationTypeEnum = Btms.Model.Ipaffs.ImportNotificationTypeEnum;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class Mrn24Gbde3Cf94H96Tar0ModifiedTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<Mrn24Gbde3Cf94H96Tar0ModifiedScenarioGenerator>(output)
{

    [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusOnDecison()
    {
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus.Decisions
            .First(d => d.Context.AlvsDecisionNumber == 2)
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }

    [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusOnPreviousDecison()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .First(d => d.Context.AlvsDecisionNumber == 1)
            .Context.DecisionComparison
            .Should().BeNull();
    }

    [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }

    [FailingFact(jiraTicket: "CDMS-234"), Trait("JiraTicket", "CDMS-234")]
    public void ShouldHave1BtmsDecision()
    {
        Client
            .GetSingleMovement()
            .Decisions.Count
            .Should().Be(1);
    }

    [Fact]
    public void ShouldHaveCorrectDecisionAuditEntries()
    {
        var notification = (ImportNotification)LoadedData
            .First(d =>
                d is { Message: ImportNotification }
            )
            .Message;

        // Assert
        var movement = Client
            .GetSingleMovement();

        var decisionWithLinkAndContext = movement.AuditEntries
            .Where(a => a is { CreatedBy: CreatedBySystem.Btms, Status: "Decision" })
            .MaxBy(a => a.Version)!;

        decisionWithLinkAndContext.Context.As<DecisionContext>()!.ImportNotifications
            .Should().NotBeNull();

        decisionWithLinkAndContext.Context.As<DecisionContext>()!.ImportNotifications!
            .Select(n => (n.Id, n.Version))
            .Should()
            .Equal([
                (notification.ReferenceNumber!, 3)
            ]);
    }

    [Fact]
    public void ShouldHave2AlvsDecisions()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Decisions
            .Count
            .Should()
            .Be(2);
    }

    [FailingFact(jiraTicket: "CDMS-234"), Trait("JiraTicket", "CDMS-234")]
    public void ShouldHaveCorrectAuditTrail()
    {
        Client
            .GetSingleMovement()
            .AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version))
            .Should()
            .Equal([
                (CreatedBySystem.Cds, "Created", 1),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Decision", 1),
                (CreatedBySystem.Alvs, "Decision", 2),
                (CreatedBySystem.Alvs, "Decision", 1)
            ]);
    }

    [Fact]
    public void ShouldHaveDecisionMatched()
    {
        var movement = Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context!.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }

    [Fact]
    public void ShouldHaveDecisionStatus()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs);
    }

    [Fact]
    public void ShouldHaveChedType()
    {
        Client
            .GetSingleMovement()
            .Status.ChedTypes
            .Should().Equal(ImportNotificationTypeEnum.Cvedp);
    }

    [Fact]
    public void ShouldBeLinked()
    {
        Client
            .GetSingleMovement()
            .Status.LinkStatus
            .Should().Be(LinkStatus.AllLinked);
    }

    [Fact]
    public async Task ShouldNotHaveExceptions()
    {
        var result = await Client
            .GetExceptions();

        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());

        (await result.GetString())
            .Should()
            .Be("[]");
    }

    [Fact]
    public void AlvsDecisionShouldHaveCorrectChecks()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.Checks
            .Should().BeEquivalentTo([
                new
                {
                    ItemNumber = 1,
                    CheckCode = "H222",
                    AlvsDecisionCode = "C03",
                    BtmsDecisionCode = "C03"
                }
            ]);
    }

    [Fact]
    public async Task AlvsDecisionShouldReturnCorrectlyFromAnalytics()
    {
        var result = await (await Client
            .GetAnalyticsDashboard(["decisionsByDecisionCode"]))
            .ToJsonDictionary();

        // TODO would be nice to deserialise this into our dataset structures from analytics... 
        result["decisionsByDecisionCode"]?["summary"]?["values"]?[
            "Btms Made Same Decision As Alvs"]?
            .GetValue<int>()
            .Should().Be(2);
    }
}
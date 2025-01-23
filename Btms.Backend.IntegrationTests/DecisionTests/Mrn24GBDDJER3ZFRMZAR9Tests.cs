using Btms.Model.Cds;
using Btms.Types.Ipaffs;
using Btms.Model.Auditing;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;
using ImportNotificationTypeEnum = Btms.Model.Ipaffs.ImportNotificationTypeEnum;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration"), Trait("Segment", "CDMS-205-Ac5")]
public class Mrn24GBDDJER3ZFRMZAR9Tests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<Mrn24GBDDJER3ZFRMZAR9ScenarioGenerator>(output)
{

    [FailingFact(jiraTicket:"CDMS-235"), Trait("JiraTicket", "CDMS-235")]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusOnDecison()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .First(d => d.Context.AlvsDecisionNumber == 2)
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }
    
    [FailingFact(jiraTicket:"CDMS-235"), Trait("JiraTicket", "CDMS-235")]
    public void ShouldHaveCorrectAlvsDecisionStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs);
    }
    
    [FailingFact(jiraTicket:"CDMS-235"), Trait("JiraTicket", "CDMS-235")]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }

    // [FailingFact(jiraTicket:"CDMS-234"), Trait("JiraTicket", "CDMS-234")]
    [Fact]
    public void ShouldHave2BtmsDecisions()
    {
        var actual = Client
            .GetSingleMovement()
            .Decisions;

        actual.Count
            .Should().Be(2);
    }

    // [FailingFact(jiraTicket:"CDMS-235"), Trait("JiraTicket", "CDMS-235")]
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
            .Distinct()
            .Count()
            .Should()
            .Be(9);
    }
    
    [Fact]
    public void ShouldHave1AlvsDecisions()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions.Count
            .Should()
            .Be(1);
    }
    
    [Fact]
    public void ShouldHave3AlvsDecisionChecks()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.Checks.Count
            .Should()
            .Be(3);
    }

    // [FailingFact(jiraTicket:"CDMS-235"), Trait("JiraTicket", "CDMS-235")]
    [Fact]
    public void ShouldHaveCorrectAuditTrail()
    {
        var actual = Client
            .GetSingleMovement()
            .AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version));

        actual.Should()
            .Equal([
                (CreatedBySystem.Cds, "Created", 1),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Decision", 1),
                (CreatedBySystem.Cds, "Updated", 2),
                (CreatedBySystem.Btms, "Decision", 2),
                (CreatedBySystem.Alvs, "Decision", 1)
            ]);
    }

    [FailingFact(jiraTicket:"CDMS-235"), Trait("JiraTicket", "CDMS-235")]
    public void ShouldHaveDecisionMatched()
    {
        var movement = Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context!.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }
    
    [Fact]
    public void ShouldHaveChedPpDecisionStatus()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.HasChedppChecks);
    }
    
    [Fact]
    public void ShoulHaveCorrectBtmsStatus()
    {
        var movement = 
            Client
                .GetSingleMovement();
            
        movement
            .BtmsStatus
            .Should().BeEquivalentTo(
                new { 
                    LinkStatus = LinkStatusEnum.AllLinked,
                    Segment = MovementSegmentEnum.Cdms205Ac5,
                    ChedTypes = (ImportNotificationTypeEnum[])[ImportNotificationTypeEnum.Chedpp]
                }
            );
    }
    
    // [Fact]
    // public void ShouldBeLinked()
    // {
    //     Client
    //         .GetSingleMovement()
    //         .BtmsStatus.LinkStatus
    //         .Should().Be(LinkStatusEnum.Linked);
    // }
    
    [Fact]
    public async Task ShouldNotHaveExceptions()
    {
        // TestOutputHelper.WriteLine("Querying for aggregated data");

        var result = await Client
            .GetExceptions();
        
        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());
        
        (await result.GetString())
            .Should()
            .Be("[]");
    }
    
    [FailingFact(jiraTicket:"CDMS-205")]
    // [Fact]
    public void AlvsDecisionShouldHaveCorrectChecks()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.Checks
            .Should().BeEquivalentTo([
                new { 
                    ItemNumber = 1,
                    CheckCode = "H219",
                    AlvsDecisionCode = "C03", 
                    BtmsDecisionCode = "C03"
                },
                new { 
                    ItemNumber = 2,
                    CheckCode = "H219",
                    AlvsDecisionCode = "C03", 
                    BtmsDecisionCode = "C03"
                },
                new { 
                    ItemNumber = 3,
                    CheckCode = "H219",
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
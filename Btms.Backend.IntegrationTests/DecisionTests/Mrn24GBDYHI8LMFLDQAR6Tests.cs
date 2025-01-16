using Btms.Common.Extensions;
using Btms.Model.Auditing;
using Btms.Model.Cds;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;
using ImportNotificationTypeEnum = Btms.Model.Ipaffs.ImportNotificationTypeEnum;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class Mrn24GBDYHI8LMFLDQAR6Tests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<Mrn24GBDYHI8LMFLDQAR6ScenarioGenerator>(output)
{
    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
    // [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusOnDecison()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .MaxBy(d => d.Context.AlvsDecisionNumber)!
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }
    
    // [Fact]
    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
    public void ShouldHaveCorrectAlvsDecisionStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs);
    }
    
    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
    // [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }

    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
    // [Fact]
    public void ShouldHave1BtmsDecision()
    {
        // Act
        var decisions = Client
            .GetSingleMovement()
            .Decisions;
        
        // Assert
        decisions.Count
            .Should().Be(1);
    }

    // [Fact]
    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
    public void ShouldHaveCorrectDecisionAuditEntries()
    {
        var movement = Client
            .GetSingleMovement();
        
        // Assert
        
        var decisionWithLinkAndContext = movement.AuditEntries
            .Where(a => a is { CreatedBy: CreatedBySystem.Btms, Status: "Decision" })
            .MaxBy(a => a.Version)!;

        decisionWithLinkAndContext
            .Should().NotBeNull();
    }
    
    [Fact]
    public void ShouldHave1AlvsDecision()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions.Count
            .Should()
            .Be(1);
    }

    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
    // [Fact]
    public void ShouldHaveCorrectAuditTrail()
    {
        // Act
        var auditTrail = Client
            .GetSingleMovement()
            .AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version))
            .Should()
            .BeEquivalentTo<(CreatedBySystem, string, int?)>([
                (CreatedBySystem.Cds, "Created", 1),
                (CreatedBySystem.Btms, "Decision", 1),
                (CreatedBySystem.Alvs, "Decision", 1)
            ]);
    }
    
    // [Fact]
    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
    public void ShouldHaveChedType()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.ChedTypes
            .Should().BeNull();
    }
    
    // [Fact]
    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
    public void ShouldNotBeLinked()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.LinkStatus
            .Should().Be(LinkStatusEnum.NotLinked);
    }
    
    // [Fact]
    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
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
    
    /// <summary>
    /// Ensures we capture the error status of the movement
    /// but will likely need to change as part of the error code work
    /// </summary>
    [Fact]
    public void ShouldBeFlaggedAsError()
    {
        var movement = 
        Client
            .GetSingleMovement();
            
        movement
            .BtmsStatus
            .Should().BeEquivalentTo(
                new { 
                    LinkStatus = LinkStatusEnum.Error,
                    LinkStatusDescription = "ALVSVAL318",
                    Status = MovementStatusEnum.Error
                }
            );
    }
    
    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
    // [Fact]
    public void AlvsDecisionShouldHaveCorrectChecks()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.Checks
            .Should().BeEquivalentTo([
                new { 
                    ItemNumber = 1,
                    CheckCode = "H220",
                    AlvsDecisionCode = "X01", 
                    BtmsDecisionCode = "X01"
                }
            ]);
    }
    
    // [Fact]
    [FailingFact(jiraTicket:"CDMS-242"), Trait("JiraTicket", "CDMS-242")]
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
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
public class Mrn24GBDEHMFC4WGXVAR7Tests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<Mrn24GBDEHMFC4WGXVAR7ScenarioGenerator>(output)
{
    [FailingFact(jiraTicket:"CDMS-232"), Trait("JiraTicket", "CDMS-232")]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusOnDecison()
    {
        Client
            .GetSingleMovement()
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
    public void ShouldHaveCorrectAlvsDecisionStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.AlvsClearanceRequestVersion1NotPresent);
    }
    
    [FailingFact(jiraTicket:"CDMS-232"), Trait("JiraTicket", "CDMS-232")]
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
        // Act
        var decisions = Client
            .GetSingleMovement()
            .Decisions;
        
        // Assert
        // This should really only be 2, but with the update logic fixed we now need to dedupe decisions
        decisions.Count
            .Should().Be(3);
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
            .Where(a => a is { CreatedBy: "Btms", Status: "Decision" })
            .MaxBy(a => a.Version)!;
        
        decisionWithLinkAndContext.Context!.ImportNotifications
            .Should().NotBeNull();
        
        decisionWithLinkAndContext.Context!.ImportNotifications!
            .Select(n => (n.Id, n.Version))
            .Should()
            .Equal([
                ( notification.ReferenceNumber!, 1 )
            ]);
    }
    
    [Fact]
    public void ShouldHave3AlvsDecisions()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions.Count
            .Should()
            .Be(3);
    }

    [FailingFact(jiraTicket:"CDMS-234"), Trait("JiraTicket", "CDMS-234")]
    public void ShouldHaveCorrectAuditTrail()
    {
        // Act
        var auditTrail = Client
            .GetSingleMovement()
            .AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version));
            
        // Assert
        auditTrail.Should()
            .BeEquivalentTo<(string, string, int?)>([
                ("Cds", "Created", 1),
                ("Btms", "Linked", null),
                ("Btms", "Decision", 1),
                ("Alvs", "Decision", 1),
                ("Btms", "Decision", 2),
                ("Alvs", "Decision", 2),
                ("Cds", "Updated", 2),
                ("Btms", "Decision", 3),
                ("Alvs", "Decision", 3),
            ]);
    }
    
    [Fact]
    public void ShouldHaveChedPPDecisionStatus()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.AlvsClearanceRequestVersion1NotPresent);
    }
    
    [Fact]
    public void ShouldHaveChedType()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.ChedTypes
            .Should().Equal(ImportNotificationTypeEnum.Cvedp);
    }
    
    [Fact]
    public void ShouldBeLinked()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.LinkStatus
            .Should().Be(LinkStatusEnum.Linked);
    }
    
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
    
    [FailingFact(jiraTicket:"CDMS-232"), Trait("JiraTicket", "CDMS-232")]
    public void AlvsDecisionShouldHaveCorrectChecks()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.Checks
            .Should().BeEquivalentTo([
                new { 
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
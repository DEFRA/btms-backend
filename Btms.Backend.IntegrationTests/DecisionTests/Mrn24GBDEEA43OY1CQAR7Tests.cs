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
public class Mrn24GBDEEA43OY1CQAR7Tests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<Mrn24GBDEEA43OY1CQAR7ScenarioGenerator>(output)
{

    [FailingFact(jiraTicket:"CDMS-205", "Has Ched PP Checks"), Trait("JiraTicket", "CDMS-205")]
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
    
    [FailingFact(jiraTicket:"CDMS-205", "Has Ched PP Checks"), Trait("JiraTicket", "CDMS-205")]
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
        // This should really only be 1, but with the update logic fixed we now need to dedupe decisions
        decisions.Count
            .Should().Be(2);
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
    public void ShouldHave2AlvsDecisions()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions.Count
            .Should()
            .Be(2);
    }

    // [Fact]
    [FailingFact(jiraTicket:"CDMS-205", "Has Ched PP Checks"), Trait("JiraTicket", "CDMS-205")]
    public void ShouldHaveCorrectAuditTrail()
    {
        // Act
        var auditTrail = Client
            .GetSingleMovement()
            .AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version));

        // Assert
        auditTrail.Should()
            .Equal([
                (CreatedBySystem.Cds, "Created", 1),
                (CreatedBySystem.Btms, "Linked", null),
                (CreatedBySystem.Btms, "Decision", 1),
                (CreatedBySystem.Cds, "Updated", 2),
                (CreatedBySystem.Btms, "Decision", 2),
                (CreatedBySystem.Alvs, "Decision", 1),
                (CreatedBySystem.Alvs, "Decision", 2)
            ]);
    }

    [FailingFact(jiraTicket:"CDMS-205", "Has Ched PP Checks"), Trait("JiraTicket", "CDMS-205")]
    public void ShouldHaveDecisionMatched()
    {
        var movement = Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context!.DecisionComparison!.DecisionMatched
            .Should().BeTrue();
    }
    
    [Fact]
    public void ShouldHaveChedPPDecisionStatus()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.HasChedppChecks);
    }
    
    [Fact]
    public void ShouldHaveChedType()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.ChedTypes
            .Should().Equal(ImportNotificationTypeEnum.Chedpp);
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
    
    [FailingFact(jiraTicket:"CDMS-205", "Has Ched PP Checks"), Trait("JiraTicket", "CDMS-205")]
    public void AlvsDecisionShouldHaveCorrectChecks()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.Checks
            .Should().BeEquivalentTo([
                new { 
                    ItemNumber = 1,
                    CheckCode = "H218",
                    AlvsDecisionCode = "C03", 
                    BtmsDecisionCode = "C03"
                },
                new { 
                    ItemNumber = 1,
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
            .GetAnalyticsDashboard(["decisionsByDecisionCode"],
                dateFrom:DateTime.MinValue, dateTo:DateTime.MaxValue))
            .ToJsonDictionary();

        // TODO would be nice to deserialise this into our dataset structures from analytics... 
        result["decisionsByDecisionCode"]!["summary"]!["values"]![
            "Has Other E9X Data Errors"]!
            .GetValue<int>()
            .Should().Be(2);
    }
}
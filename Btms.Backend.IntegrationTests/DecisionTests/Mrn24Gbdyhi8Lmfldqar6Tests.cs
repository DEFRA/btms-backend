using Btms.Model.Auditing;
using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration"), Trait("Segment", "CDMS-249")]
public class ClearanceRequestWithNoDocuments(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<Mrn24Gbdyhi8Lmfldqar6ScenarioGenerator>(output)
{
    [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusOnDecison()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .MaxBy(d => d.Context.AlvsDecisionNumber)?
            .Context.DecisionComparison?.DecisionMatched
            .Should().BeFalse();
    }
    
    [Fact]
    public void ShouldHaveCorrectAlvsDecisionStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison?.DecisionStatus
            .Should().Be(DecisionStatusEnum.NoAlvsDecisions);
    }
    
    [Fact]
    public void ShouldHaveCorrectAlvsDecisionMatchedStatusAtGlobalLevel()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionMatched
            .Should().BeFalse();
    }

    [Fact]
    public void ShouldHaveNoBtmsDecisions()
    {
        var decisions = Client
            .GetSingleMovement()
            .Decisions;
        
        decisions.Count
            .Should().Be(0);
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

    [Fact]
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
                (CreatedBySystem.Alvs, "Decision", 1)
            ]);
    }
    
    [Fact]
    public void ShouldHaveChedType()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.ChedTypes
            .Should().BeEmpty();
    }
    
    [Fact]
    public void ShouldNotBeLinked()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.LinkStatus
            .Should().Be(LinkStatusEnum.NoLinks);
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
    
    /// <summary>
    /// Ensures we capture the error status of the movement
    /// but will likely need to change as part of the error code work
    /// </summary>
    [Fact]
    public void ShouldHaveCorrectBtmsStatus()
    {
        var movement = 
        Client
            .GetSingleMovement();
            
        movement
            .BtmsStatus
            .Should().BeEquivalentTo(
                new { 
                    LinkStatus = LinkStatusEnum.NoLinks,
                    LinkStatusDescription = "ALVSVAL318",
                    Segment = MovementSegmentEnum.Cdms249
                }
            );
    }
    
    [Fact]
    public void AlvsDecisionShouldHaveCorrectChecks()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison?.Checks
            .Should().BeEmpty();
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
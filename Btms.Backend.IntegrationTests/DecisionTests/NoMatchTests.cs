using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Model.Cds;
using FluentAssertions;
using Humanizer;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class NoMatchTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrNoMatchSingleItemWithDecisionScenarioGenerator>(output)
{
    
    [Fact]
    public void ShouldNotHaveLinked()
    {
        Client
            .GetSingleMovement()
            .BtmsStatus.LinkStatus
            .Should().Be("Not Linked");
    }
    
    [Fact]
    public void ShouldHaveAlvsDecision()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions.Count
            .Should().Be(1);
    }
    
    [Fact]
    public void AlvsDecisionShouldHaveCorrectChecks()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.Checks
            .Should().BeEquivalentTo([
                new { 
                    ItemNumber = 1,
                    CheckCode = "H222",
                    AlvsDecisionCode = "H01", 
                    BtmsDecisionCode = "X00"
                },
                new {
                    ItemNumber = 1,
                    CheckCode = "H224",
                    AlvsDecisionCode = "H01", 
                    BtmsDecisionCode = "X00"
                    
                }
            ]);
    }
    
    [Fact]
    public void ShouldHaveDecisionStatus()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.InvestigationNeeded);
    }
    
    [Fact]
    public void ShouldHaveDecisionAuditChecks()
    {
        Client
            .GetSingleMovement()
            .SingleBtmsDecisionAuditEntry()
            .Context?.DecisionComparison?.Checks
            .Should().NotBeNull();
    }
    
    [Fact]
    public void ShouldNotHaveDecisionAuditNotifications()
    {
        Client
            .GetSingleMovement()
            .SingleBtmsDecisionAuditEntry()
            .Context?.ImportNotifications
            .Should().BeEmpty();
    }
    
    [Fact]
    public void ShouldHaveDecisionMatchedFalse()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context!.DecisionComparison!.DecisionMatched
            .Should()
            .BeFalse();
    }
}
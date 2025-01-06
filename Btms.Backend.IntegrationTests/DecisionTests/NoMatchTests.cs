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
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement.BtmsStatus.LinkStatus.Should().Be("Not Linked");
    }
    
    [Fact]
    public void ShouldHaveAlvsDecision()
    {
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement.AlvsDecisionStatus.Decisions.Count.Should().Be(1);
    }
    
    [Fact]
    public void ShouldHaveDecisionStatus()
    {
        
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement.AlvsDecisionStatus.DecisionStatus.Should().Be(DecisionStatusEnum.InvestigationNeeded);
    }
    
    [Fact]
    public void ShouldHaveDecisionAuditChecks()
    {
        var auditEntry = Client
            .GetSingleMovement()
            .SingleBtmsDecisionAuditEntry();

        auditEntry.Context?.Checks.Should().NotBeNull();
    }
    
    [Fact]
    public void ShouldNotHaveDecisionAuditNotifications()
    {
        
        var auditEntry = Client
            .GetSingleMovement()
            .SingleBtmsDecisionAuditEntry();

        auditEntry.Context?.ImportNotifications.Should().BeEmpty();
    }
    
    [Fact]
    public void ShouldHaveDecisionMatchedFalse()
    {
        
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .Context!
            .DecisionMatched
            .Should()
            .BeFalse();
    }
}
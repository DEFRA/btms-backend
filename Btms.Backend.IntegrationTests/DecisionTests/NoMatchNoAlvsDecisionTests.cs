using Btms.Model;
using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class NoMatchNoAlvsDecisionTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrNoMatchNoDecisionScenarioGenerator>(output)
{
    
    [Fact]
    public void ShouldHaveNotificationRelationships()
    {
        
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement.BtmsStatus.LinkStatus.Should().Be("Not Linked");
    }
    
    [Fact]
    public void ShouldHaveDecisionStatus()
    {
        
        // Assert
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.NoAlvsDecisions);
    }
    
    [Fact]
    public void ShouldHaveDecisionMatched()
    {   
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .Context!
            .DecisionComparison!
            .DecisionMatched
            .Should()
            .BeFalse();
    }
}
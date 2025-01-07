using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class OutOfSequenceAlvsDecision(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<OutOfSequenceAlvsDecisionScenarioGenerator>(output)
{
    
    [Fact]
    public void ShouldHave2AlvsDecisions()
    {
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .Decisions
            .Count
            .Should()
            .Be(2);
    }
    
    [Fact]
    public void ShouldHaveCorrectDecisionNumbers()
    {
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .Decisions
            .Select(d => d.Context.AlvsDecisionNumber)
            .Should()
            .Equal(2, 1);
    }
    
    [Fact]
    public void ShouldHaveVersionNotCompleteDecisionStatus()
    {
        
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .DecisionStatus
            .Should()
            .Be(DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs);
    }
    
    [Fact]
    public void ShouldHavePairedAlvsDecisions()
    {
        
        // Assert
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .Decisions
            .Count(d => d.Context.Paired)
            .Should().Be(1);
    }
    
    [Fact]
    public void ShouldHavePairedBtmsDecisions()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Decisions
            .Select(d => (d.Context.AlvsDecisionNumber, d.Context.BtmsDecisionNumber))
            .Should().Equal(
                (2,2),
                (1, null));
    }
}
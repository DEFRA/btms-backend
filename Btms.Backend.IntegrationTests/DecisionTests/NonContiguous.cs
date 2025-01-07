using Btms.Backend.IntegrationTests.Helpers;
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
public class NonContiguous(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrNonContiguousDecisionsScenarioGenerator>(output)
{
    
    [Fact]
    public void ShouldHave2AlvsDecisions()
    {
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
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .Decisions
            .Select(d => d.Context.AlvsDecisionNumber)
            .Should()
            .Equal(1, 3);
    }
    
    [Fact]
    public void ShouldHaveVersionNotCompleteDecisionStatus()
    {
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .DecisionStatus
            .Should()
            .Be(DecisionStatusEnum.AlvsDecisionVersionsNotComplete);
    }
    
    [Fact]
    public void ShouldHavePairedAlvsDecisions()
    {
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .Decisions
            .Count(d => d.Context.Paired)
            .Should().Be(1);
    }
    
    [Fact]
    public void ShouldHave1BtmsDecision()
    {
        var movement = Client
            .GetSingleMovement();

        movement
            .Decisions
            .Count
            .Should()
            .Be(1);
    }
    
    [Fact]
    public void ShouldHavePairedBtmsDecisions()
    {
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus
            .Decisions
            .Select(d => (d.Context.AlvsDecisionNumber, d.Context.BtmsDecisionNumber))
            .Should().Equal((1,null), (3,1));
    }
}
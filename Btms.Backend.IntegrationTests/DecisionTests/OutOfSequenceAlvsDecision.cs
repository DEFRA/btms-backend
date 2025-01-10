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
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions.Count
            .Should().Be(2);
    }
    
    [Fact]
    public void ShouldHaveCorrectDecisionNumbers()
    {
        // Assert
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .Select(d => d.Context.AlvsDecisionNumber)
            .Should().Equal(2, 1);
    }
    
    [Fact]
    public void ShouldHaveVersionNotCompleteDecisionStatus()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should().Be(DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs);
    }
    
    [Fact]
    public void ShouldHavePairedAlvsDecisions()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .Select(d => d.Context.DecisionComparison?.Paired)
            // .Count(d => d.Context.DecisionComparison!.Paired)
            .Should().Equal(true, null);
    }
    
    [Fact]
    public void ShouldHavePairedBtmsDecisions()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Decisions
            .Select(d => (d.Context.AlvsDecisionNumber, d.Context.DecisionComparison?.BtmsDecisionNumber))
            .Should().Equal(
                (2,2),
                (1, null));
    }
}
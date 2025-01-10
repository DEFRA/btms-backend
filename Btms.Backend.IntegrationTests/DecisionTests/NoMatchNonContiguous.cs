using Btms.Backend.IntegrationTests.Helpers;
using Btms.Common.Extensions;
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
public class NoMatchNonContiguous(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrNoMatchNonContiguousDecisionsScenarioGenerator>(output)
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
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
            .Should()
            .Be(DecisionStatusEnum.NoImportNotificationsLinked);
    }
    
    [Fact]
    public void ShouldHavePairedAlvsDecision()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus.Decisions
            .Select(d => d.Context.DecisionComparison?.Paired)
            // .Count(d => d.Context.DecisionComparison is { Paired: true })
            .Should().Equal(null, true);
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
    public void ShouldHave1BtmsDecision()
    {
        Client
            .GetSingleMovement()
            .Decisions.Count
            .Should().Be(1);
    }
    
    [Fact]
    public void ShouldHavePairedBtmsDecisions()
    {
        var movement = Client
            .GetSingleMovement();

        movement
            .AlvsDecisionStatus.Decisions
            .Select(d => (d.Context.AlvsDecisionNumber, d.Context.DecisionComparison?.BtmsDecisionNumber))
            .Should().Equal((1,null), (3,1));
    }
}
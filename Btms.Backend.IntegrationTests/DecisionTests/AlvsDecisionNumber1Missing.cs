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
public class AlvsDecisionNumber1Missing(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrDecisionWithoutV1ScenarioGenerator>(output)
{
    
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
    public void ShouldHave1AlvsDecision()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Decisions
            .Count
            .Should()
            .Be(1);
    }
    
    [Fact]
    public void ShouldHaveCorrectDecisionNumbers()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Decisions
            .Select(d => d.Context.AlvsDecisionNumber)
            .Should()
            .Equal(2);
    }
    
    [Fact]
    public void ShouldHaveVersionNotCompleteDecisionStatus()
    {
        Client
            .GetSingleMovement()
            .AlvsDecisionStatus
            .Context.DecisionComparison!
            .DecisionStatus
            .Should()
            .Be(DecisionStatusEnum.NoImportNotificationsLinked);
    }
}
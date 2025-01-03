using Btms.Model;
using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class NoMatchNoAlvsDecisionTests(ITestOutputHelper output)
    : BaseTest<CrNoMatchNoDecisionScenarioGenerator>(output)
{
    
    [Fact]
    public void ShouldHaveNotificationRelationships()
    {
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

        movement.BtmsStatus.LinkStatus.Should().Be("Not Linked");
    }
    
    [Fact]
    public void ShouldHaveDecisionStatus()
    {
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

        movement.AlvsDecisionStatus.DecisionStatus.Should().Be(DecisionStatusEnum.InvestigationNeeded);
    }
    
    [Fact]
    public void ShouldHaveDecisionMatched()
    {
        // var res =  Client.AsJsonApiClient()
        //     .Get("api/movements");
        
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

        movement
            .AlvsDecisionStatus
            .Context!
            .DecisionMatched
            .Should()
            .BeFalse();
    }
}
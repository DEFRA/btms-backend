using Btms.Backend.IntegrationTests.Helpers;
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
public class NonContiguous(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrNoMatchNonContiguousDecisionsScenarioGenerator>(output)
{

    [Fact]
    public void ShouldHave2AlvsDecisions()
    {
        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

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
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

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

        // Assert
        var movement = Client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();

        movement
            .AlvsDecisionStatus
            .Context.DecisionComparison!.DecisionStatus
            .Should()
            .Be(DecisionStatusEnum.NoImportNotificationsLinked);
    }
}
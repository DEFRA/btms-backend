using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.FinalisationTests;

[Trait("Category", "Integration")]
public class Cleared(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<Mrn24GBDE8OLVKZXSYAR1ScenarioGenerator>(output)
{

    [Fact]
    public void FinalisedShouldBeSet()
    {
        Client
            .GetSingleMovement()
            .Finalised
            .Should().NotBeNull();
    }
    
    [Fact]
    public void FinalisationFinalStateShouldBeCorrect()
    {
        Client
            .GetSingleMovement()
            .Finalisation!.FinalState
            .Should().Be(FinalState.Cleared);
    }

}
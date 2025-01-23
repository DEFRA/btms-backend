using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.FinalisationTests;

[Trait("Category", "Integration")]
public class Cleared(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<ChedASimpleMatchScenarioGenerator>(output)
{

    [Fact]
    public void FinalisedShouldBeSet()
    {
        Client
            .GetSingleMovement()
            .Finalised
            .Should().NotBeNull();
    }

}
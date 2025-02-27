using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class ManyItemTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<CrNoMatchScenarioGenerator>(output)
{

    [Fact]
    public void ShouldHaveOneChedType()
    {
        // Act
        var movementResource = Client
            .GetSingleMovement()
            .Status.ChedTypes!.Count()
            .Should().Be(1);
    }
}
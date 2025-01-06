using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
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
        var movementResource = Client.AsJsonApiClient()
            .Get("api/movements")
            // .Data
            .GetResourceObjects<Movement>()
            .Single()
            .BtmsStatus.ChedTypes!.Count()
            .Should().Be(1);
    }
    
}
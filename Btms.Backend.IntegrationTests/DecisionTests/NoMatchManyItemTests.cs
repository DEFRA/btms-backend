using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class ManyItemTests(InMemoryScenarioApplicationFactory<CrNoMatchScenarioGenerator> factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "DecisionTests"), IClassFixture<InMemoryScenarioApplicationFactory<CrNoMatchScenarioGenerator>>
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
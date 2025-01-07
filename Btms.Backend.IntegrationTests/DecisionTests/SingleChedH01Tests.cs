using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model;
using Btms.Types.Alvs;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]

public class SingleChedH01Tests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<SimpleMatchScenarioGenerator>(output)
{
    [Fact]
    public void SingleChed_ShouldHaveH01CheckValues()
    {
        // Arrange
        var loadedData = LoadedData;

        var chedPClearanceRequest = (AlvsClearanceRequest)loadedData.Single(d =>
                d is { Message: AlvsClearanceRequest })
            .Message;

        // Act
        var chedPMovement = Client.AsJsonApiClient()
            .GetById(chedPClearanceRequest!.Header!.EntryReference!, "api/movements").GetResourceObject<Movement>();

        // Assert
        chedPMovement.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!.Single().Checks!.Single()
            .DecisionCode.Should().Be("H01");
    }
}
using System.Diagnostics.CodeAnalysis;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.SyncJob;
using Btms.Backend.IntegrationTests.JsonApiClient;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Btms.Backend.IntegrationTests.Extensions;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using Btms.Types.Alvs;
using TestDataGenerator.Scenarios;
using Json.More;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class DecisionTests(TestDataGeneratorFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "DecisionTests"), IClassFixture<TestDataGeneratorFactory>
{
    
    [Fact]
    public async Task SimpleChedPScenario()
    {
        // Act
        var loadedData = await factory.GenerateAndLoadTestData(Client, "One");
        

        // Assert

        var chedPMovement = loadedData.Single(d =>
            d is { generator: ChedPSimpleMatchScenarioGenerator, message: AlvsClearanceRequest })
            .message as AlvsClearanceRequest;
        
        var jsonClientResponse = Client.AsJsonApiClient().GetById(chedPMovement!.Header!.EntryReference!, "api/movements");

        jsonClientResponse.Should().NotBeNull();
    }
}
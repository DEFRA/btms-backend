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
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Btms.Types.Alvs;
using TestDataGenerator.Scenarios;
using Json.More;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Scenarios.ChedP;
using Xunit;
using Xunit.Abstractions;
using ImportNotification = Btms.Types.Ipaffs.ImportNotification;

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
            .Status.ChedTypes!.Count().Should().Be(1);
    }
    
}
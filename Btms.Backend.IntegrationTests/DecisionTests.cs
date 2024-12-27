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
    public async Task SimpleChedPScenario_ShouldBeLinkedAndMatchDecision()
    {
        // Arrange
        var loadedData = await factory.GenerateAndLoadTestData(Client, "One");
        
        var chedPMovement = loadedData.Single(d =>
                d is { generator: ChedPSimpleMatchScenarioGenerator, message: AlvsClearanceRequest })
            .message as AlvsClearanceRequest;
        
        // Act
        var jsonClientResponse = Client.AsJsonApiClient().GetById(chedPMovement!.Header!.EntryReference!, "api/movements");
        
        // Assert
        jsonClientResponse.Should().NotBeNull();
        jsonClientResponse.Data.Relationships!.Count.Should().Be(1);
        
        var movement = jsonClientResponse.GetResourceObject<Movement>();
        movement.Decisions.Count.Should().Be(2);
        movement.AlvsDecisionStatus.Decisions.Count.Should().Be(1);
        
        movement.AlvsDecisionStatus.Decisions
            .First()
            .Context.DecisionMatched
            .Should().BeTrue();

        movement.AuditEntries
            .Select(a => (a.CreatedBy, a.Status, a.Version))
            .Should()
            .Equal([
                ("Cds", "Created", 1),
                ("Btms", "Decision", 1),
                ("Btms", "Linked", null),
                ("Btms", "Decision", 2),
                ("Alvs", "Decision", 1)
            ]);
        
        // TODO : for some reason whilst jsonClientResponse contains the notification relationship, movement doesn't!
        // movement.Relationships.Notifications.Data.Count.Should().Be(1);
    }
}
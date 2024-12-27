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
public class ChedPDecisionTests(TestDataGeneratorFixture fixture, ITestOutputHelper testOutputHelper) : IClassFixture<TestDataGeneratorFixture>
    // : BaseApiTests(fixture, testOutputHelper, "ChedPDecisionTests"), IClassFixture<TestDataGeneratorFixture>
{
    
    [Fact]
    public void SimpleChedPScenario_ShouldBeLinkedAndMatchDecision()
    {
        // true.Should().BeFalse();
        testOutputHelper.WriteLine("SimpleChedPScenario_ShouldBeLinkedAndMatchDecision");
        testOutputHelper.WriteLine(fixture.ToString());
        
        // // Arrange 
        // var chedPMovement = fixture.LoadedData!.Single(d =>
        //         d is { generator: ChedPSimpleMatchScenarioGenerator, message: AlvsClearanceRequest })
        //     .message as AlvsClearanceRequest;
        //
        // // Act
        // var jsonClientResponse = Client.AsJsonApiClient().GetById(chedPMovement!.Header!.EntryReference!, "api/movements");
        //
        //
        // // Assert
        // jsonClientResponse.Should().NotBeNull();
        // jsonClientResponse.Data.Relationships!.Count.Should().Be(1);
        //
        // var movement = jsonClientResponse.GetResourceObject<Movement>();
        // movement.Decisions.Count.Should().Be(2);
        // movement.AlvsDecisions.Count.Should().Be(1);
        //
        // movement.AlvsDecisions.First().Context.DecisionMatched.Should().BeTrue();
        //
        // // TODO : for some reason whilst jsonClientResponse contains the notification relationship, movement doesn't!
        // // movement.Relationships.Notifications.Data.Count.Should().Be(1);
        
    }
}
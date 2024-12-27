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
        
        // Arrange
        await factory.GenerateAndLoadTestData(Client);
        // await factory.ClearDb(Client);
        // await factory.InsertScenario<ChedPSimpleMatchScenarioGenerator>();

        
        // // Arrange
        // await factory.ClearDb(Client);
        // // await factory.InsertScenario<ChedPSimpleMatchScenarioGenerator>();
        
        //
        // // Act
        // var period = SyncPeriod.All;
        // var rootFolder = "GENERATED-ONE";
        // await MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        // {
        //     SyncPeriod = period,
        //     RootFolder = rootFolder
        // });
        //
        // await MakeSyncNotificationsRequest(new SyncNotificationsCommand()
        // {
        //     SyncPeriod = period,
        //     RootFolder = rootFolder
        // });
        //
        // await MakeSyncDecisionsRequest(new SyncDecisionsCommand()
        // {
        //     SyncPeriod = period,
        //     RootFolder = rootFolder
        // });

        // Assert
        // TODO - think this ID is predicatable but TBC!
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/movements");
        
        // 23GB9999021224000001
        var ids = jsonClientResponse.Data.Select(d => d.Id);
        Factory.TestOutputHelper.WriteLine("IDs retrieved from movements API call {0}", ids);

        ids.Should().NotBeEmpty();
        // jsonClientResponse.Data.First()
        //     .Relationships
        //     .Values
        //     
        //     .Where(x => x.Relationships is not null)
        //     .SelectMany(x => x.Relationships!)
        //     .Any(x => x.Value is { Links: not null })
        //     .Should().Be(false);
    }
}
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
using Json.More;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class DecsionTests(ScenarioApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<ScenarioApplicationFactory>
{
    
    [Fact]
    public async Task SimpleChedPScenario()
    {
        // Arrange
        await factory.ClearDb(Client);

        // Act
        // await MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        // {
        //     SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        // });

        // Assert
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/movements");
        jsonClientResponse.Data
            .Where(x => x.Relationships is not null)
            .SelectMany(x => x.Relationships!)
            .Any(x => x.Value is { Links: not null })
            .Should().Be(false);
    }
}
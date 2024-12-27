using Btms.Business.Commands;
using Btms.Backend.IntegrationTests.JsonApiClient;
using FluentAssertions;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Btms.Backend.IntegrationTests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class GmrTests :
    IClassFixture<Fixture>, IAsyncLifetime
{
    private readonly BtmsClient? _client;
    private IIntegrationTestsFixture _fixture;

    public GmrTests(Fixture fixture, ITestOutputHelper testOutputHelper)
    {
        fixture.TestOutputHelper = testOutputHelper;
        fixture.DatabaseName = "GmrTests";
        _client = new BtmsClient(null);
        // _client = fixture.CreateBtmsClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        // _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        // var credentials = "IntTest:Password";
        // var credentialsAsBytes = Encoding.UTF8.GetBytes(credentials.ToCharArray());
        // var encodedCredentials = Convert.ToBase64String(credentialsAsBytes);
        // _client.DefaultRequestHeaders.Authorization =
        //     new AuthenticationHeaderValue(BasicAuthenticationDefaults.AuthenticationScheme, encodedCredentials);
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _client!.ClearDb();

        await _client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void FetchSingleGmrTest()
    {
        //Act
        var jsonClientResponse = _client!.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        // Assert
        jsonClientResponse.Data.Relationships?["customs"]?.Links?.Self.Should().Be("/api/gmr/:id/relationships/import-notifications");
        jsonClientResponse.Data.Relationships?["customs"]?.Links?.Related.Should().Be("/api/import-notifications/:id");

        jsonClientResponse.Data.Relationships?["customs"]?.Data.ManyValue?[0].Id.Should().Be("56GB123456789AB043");
        jsonClientResponse.Data.Relationships?["customs"]?.Data.ManyValue?[0].Type.Should().Be("import-notifications");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
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
    IClassFixture<IntegrationTestsApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient client;

    public GmrTests(IntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        factory.TestOutputHelper = testOutputHelper;
        factory.DatabaseName = "GmrTests";
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var credentials = "IntTest:Password";
        var credentialsAsBytes = Encoding.UTF8.GetBytes(credentials.ToCharArray());
        var encodedCredentials = Convert.ToBase64String(credentialsAsBytes);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(BasicAuthenticationDefaults.AuthenticationScheme, encodedCredentials);
    }

    public async Task InitializeAsync()
    {
        await IntegrationTestsApplicationFactory.ClearDb(client);

        await MakeSyncGmrsRequest(new SyncGmrsCommand
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
        var jsonClientResponse = client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        // Assert
        jsonClientResponse.Data.Relationships?["customs"]?.Links?.Self.Should().Be("/api/gmr/:id/relationships/import-notifications");
        jsonClientResponse.Data.Relationships?["customs"]?.Links?.Related.Should().Be("/api/import-notifications/:id");

        jsonClientResponse.Data.Relationships?["customs"]?.Data.ManyValue?[0].Id.Should().Be("56GB123456789AB043");
        jsonClientResponse.Data.Relationships?["customs"]?.Data.ManyValue?[0].Type.Should().Be("import-notifications");
    }

    private Task<HttpResponseMessage> MakeSyncGmrsRequest(SyncGmrsCommand command)
    {
        return PostCommand(command, "/sync/gmrs");
    }

    private Task<HttpResponseMessage> PostCommand<T>(T command, string uri)
    {
        var jsonData = JsonSerializer.Serialize(command);
        HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        return client.PostAsync(uri, content);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
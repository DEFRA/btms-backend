using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class GmrTests :
    IClassFixture<ApplicationFactory>, IAsyncLifetime
{
    private readonly BtmsClient _client;
    private IIntegrationTestsApplicationFactory _factory;

    public GmrTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        factory.TestOutputHelper = testOutputHelper;
        factory.DatabaseName = "GmrTests";
        _client = factory.CreateBtmsClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        // _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        // var credentials = "IntTest:Password";
        // var credentialsAsBytes = Encoding.UTF8.GetBytes(credentials.ToCharArray());
        // var encodedCredentials = Convert.ToBase64String(credentialsAsBytes);
        // _client.DefaultRequestHeaders.Authorization =
        //     new AuthenticationHeaderValue(BasicAuthenticationDefaults.AuthenticationScheme, encodedCredentials);
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _client.ClearDb();

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
        var jsonClientResponse = _client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

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
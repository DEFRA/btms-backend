using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class GmrTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<ApplicationFactory>
{
    [Fact]
    public async Task FetchSingleGmrTest()
    {
        await Client.ClearDb();
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });
        
        //Act
        var jsonClientResponse = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        // Assert
        jsonClientResponse.Data.Relationships?["customs"]?.Links?.Self.Should().Be("/api/gmr/:id/relationships/import-notifications");
        jsonClientResponse.Data.Relationships?["customs"]?.Links?.Related.Should().Be("/api/import-notifications/:id");

        jsonClientResponse.Data.Relationships?["customs"]?.Data.ManyValue?[0].Id.Should().Be("56GB123456789AB043");
        jsonClientResponse.Data.Relationships?["customs"]?.Data.ManyValue?[0].Type.Should().Be("import-notifications");
    }
}
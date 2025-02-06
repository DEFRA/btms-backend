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
    public async Task GmrImport_ShouldFindGmr()
    {
        await Client.ClearDb();
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        var document = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        document.Data.Id.Should().Be("GMRAPOQSPDUG");
    }
}
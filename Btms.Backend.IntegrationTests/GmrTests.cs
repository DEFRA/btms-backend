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
    public async Task GmrImport_ShouldCreateAndThenUpdate()
    {
        await Client.ClearDb();
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        var document = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        document.Data.Id.Should().Be("GMRAPOQSPDUG");
        document.Data.Attributes?["state"]?.ToString().Should().Be("Finalised");

        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "Linking"
        });

        document = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        document.Data.Id.Should().Be("GMRAPOQSPDUG");
        document.Data.Attributes?["state"]?.ToString().Should().Be("Embarked");
    }

    [Fact]
    public async Task GmrImport_PreservesLocalDateTimes()
    {
        await Client.ClearDb();
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        var result = await Client.AsHttpClient().GetStringAsync("api/gmrs/GMRAPOQSPDUG");

        // The exact input provided by HMRC should be retained
        result.Should().Contain("\"departsAt\": \"2024-11-11T00:25\"");

        var gmr = Factory.GetDbContext().Gmrs
            .FirstOrDefault(x => x.Id != null && x.Id.Equals("GMRAPOQSPDUG", StringComparison.OrdinalIgnoreCase));

        // Strong DateTime should stay as Unspecified as that is how incoming values are deserialized and if it were
        // Local it would serialize differently from the locale of this API.
        gmr.Should().NotBeNull();
        gmr?.PlannedCrossing.Should().NotBeNull();
        gmr?.PlannedCrossing?.DepartsAt.GetValueOrDefault().Kind.Should().Be(DateTimeKind.Unspecified);
    }
}
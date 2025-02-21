using Btms.Backend.IntegrationTests.Helpers;
using Btms.Consumers.Extensions;
using Btms.Types.Gvms;
using FluentAssertions;
using TestDataGenerator;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

public class GmrTests : BaseApiTests, IClassFixture<ApplicationFactory>
{
    public GmrTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
        factory.InternalQueuePublishWillBlock = true;
    }

    [Fact]
    public async Task GmrImport_ShouldCreateAndThenUpdate()
    {
        await Client.ClearDb();
        await LoadData<SmokeTestScenarioGenerator>(x => x is SearchGmrsForDeclarationIdsResponse);

        var document = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        document.Data.Id.Should().Be("GMRAPOQSPDUG");
        document.Data.Attributes?["state"]?.ToString().Should().Be("Finalised");

        await LoadData<LinkingScenarioGenerator>(x => x is SearchGmrsForDeclarationIdsResponse);

        document = Client.AsJsonApiClient().GetById("GMRAPOQSPDUG", "api/gmrs");

        document.Data.Id.Should().Be("GMRAPOQSPDUG");
        document.Data.Attributes?["state"]?.ToString().Should().Be("Embarked");
    }

    [Fact]
    public async Task GmrImport_PreservesLocalDateTimes()
    {
        await Client.ClearDb();
        await LoadData<SmokeTestScenarioGenerator>(x => x is SearchGmrsForDeclarationIdsResponse);

        var result = await Client.AsHttpClient().GetStringAsync("api/gmrs/GMRAPOQSPDUG");

        // The exact input provided by HMRC should be retained, which does not include
        // seconds. The assertion below includes seconds as that is what BTMS does by
        // default. Do we want to honour the HMRC spec completely?
        result.Should().Contain("\"departsAt\": \"2024-11-11T00:25:00\"");

        var gmr = Factory.GetDbContext().Gmrs
            .FirstOrDefault(x => x.Id != null && x.Id.Equals("GMRAPOQSPDUG", StringComparison.OrdinalIgnoreCase));

        // Strong DateTime should stay as Unspecified as that is how incoming values are deserialized and if it were
        // Local it would serialize differently from the locale of this API.
        gmr.Should().NotBeNull();
        gmr?.PlannedCrossing.Should().NotBeNull();
        gmr?.PlannedCrossing?.DepartsAt.GetValueOrDefault().Kind.Should().Be(DateTimeKind.Unspecified);
    }

    private async Task LoadData<T>(Func<object, bool>? filter = null) where T : ScenarioGenerator
    {
        var testGeneratorFixture = new TestGeneratorFixture(Factory.TestOutputHelper);
        var generatorResult = testGeneratorFixture.GenerateTestData<T>();
        var messages = generatorResult.Select(x => x.Message);
        messages = filter != null ? messages.Where(filter) : messages;

        await Factory.Services.PushToConsumers(Factory.TestOutputHelper.GetLogger<GmrTests>(), messages);
    }
}
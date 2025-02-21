using Btms.Backend.IntegrationTests.Helpers;
using Btms.Consumers.Extensions;
using TestDataGenerator;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Btms.Backend.IntegrationTests;

public abstract class BaseApiTests
{
    private BtmsClient? _client;

    protected BtmsClient Client => _client ??= Factory.CreateBtmsClient();

    protected IIntegrationTestsApplicationFactory Factory { get; }

    protected async Task ClearDb()
    {
        await Client.ClearDb();
    }

    protected BaseApiTests(IIntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper, string databaseName = "SmokeTests")
    {
        Factory = factory;
        Factory.TestOutputHelper = testOutputHelper;
        Factory.DatabaseName = databaseName;
    }
    
    protected async Task LoadData<T>(Func<object, bool>? filter = null) where T : ScenarioGenerator
    {
        var testGeneratorFixture = new TestGeneratorFixture(Factory.TestOutputHelper);
        var generatorResult = testGeneratorFixture.GenerateTestData<T>();
        var messages = generatorResult.Select(x => x.Message);
        messages = filter != null ? messages.Where(filter) : messages;

        await Factory.Services.PushToConsumers(Factory.TestOutputHelper.GetLogger<BaseApiTests>(), messages);
    }
}
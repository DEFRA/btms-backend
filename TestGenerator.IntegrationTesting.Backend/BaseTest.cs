using TestDataGenerator;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace TestGenerator.IntegrationTesting.Backend;

public abstract class BaseTest<T> : IClassFixture<TestGeneratorFixture>, IClassFixture<BackendFixture>
    where T : ScenarioGenerator
{
    protected readonly BtmsClient Client;
    protected readonly TestGeneratorFixture TestGeneratorFixture;
    protected readonly BackendFixture BackendFixture;
    // internal readonly BackendGeneratorFixture<T> BackendGeneratorFixture;
    protected readonly ITestOutputHelper TestOutputHelper;

    protected readonly List<GeneratedResult> LoadedData;
    protected BaseTest(ITestOutputHelper testOutputHelper,
        // BackendGeneratorFixture<T> backendGeneratorFixture
        TestGeneratorFixture testGeneratorFixture,
        BackendFixture backendFixture
    )
    {
        TestGeneratorFixture = testGeneratorFixture;
        BackendFixture = backendFixture;
        
        // Client = backendGeneratorFixture.Backend.BtmsClient;
        // TestGeneratorFixture = backendGeneratorFixture.TestGenerator;
        // BackendFixture = backendGeneratorFixture.Backend;
        TestOutputHelper = testOutputHelper;
        
        backendFixture.TestOutputHelper = testOutputHelper;
        backendFixture.DatabaseName = GetType().Name;
        backendFixture.Init(GetType().Name);
        
        Client = backendFixture.BtmsClient;

        var data = TestGeneratorFixture.GenerateTestData<T>();
        LoadedData = BackendFixture
            .LoadTestData(data)
            .GetAwaiter()
            .GetResult();
    }

    protected async Task ClearDb()
    {
        await Client.ClearDb();
    }
}
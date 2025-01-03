using Microsoft.Extensions.DependencyInjection;
using TestDataGenerator;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace TestGenerator.IntegrationTesting.Backend;

public abstract class BaseTest<T> // : IClassFixture<TestGeneratorFixture>, IClassFixture<BackendFixture>
    where T : ScenarioGenerator
{
    protected readonly BtmsClient Client;
    protected readonly TestGeneratorFixture TestGeneratorFixture;
    protected readonly BackendFixture BackendFixture;
    // internal readonly BackendGeneratorFixture<T> BackendGeneratorFixture;
    protected readonly ITestOutputHelper TestOutputHelper;

    protected readonly List<GeneratedResult> LoadedData;
    protected BaseTest(
        ITestOutputHelper testOutputHelper
    )
    {
        TestOutputHelper = testOutputHelper;
        
        TestGeneratorFixture = new TestGeneratorFixture();
        BackendFixture = new BackendFixture(testOutputHelper, GetType().Name);
        
        // BackendFixture.TestOutputHelper = testOutputHelper;
        // BackendFixture.DatabaseName = GetType().Name;
        // BackendFixture.Init(GetType().Name);
        
        Client = BackendFixture.BtmsClient;

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
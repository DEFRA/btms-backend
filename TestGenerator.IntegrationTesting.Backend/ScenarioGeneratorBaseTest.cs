using Microsoft.Extensions.DependencyInjection;
using TestDataGenerator;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace TestGenerator.IntegrationTesting.Backend;

public abstract class ScenarioGeneratorBaseTest<T>
    where T : ScenarioGenerator
{
    protected readonly BtmsClient Client;
    protected readonly ITestOutputHelper TestOutputHelper;
    
    protected readonly List<GeneratedResult> LoadedData;
    protected ScenarioGeneratorBaseTest(
        ITestOutputHelper testOutputHelper
    )
    {
        TestOutputHelper = testOutputHelper;
        
        var testGeneratorFixture = new TestGeneratorFixture(testOutputHelper);
        var backendFixture = new BackendFixture(testOutputHelper, GetType().Name);
        
        Client = backendFixture.BtmsClient;

        var data = testGeneratorFixture
            .GenerateTestData<T>();
        
        LoadedData = backendFixture
            .LoadTestData(data)
            .GetAwaiter()
            .GetResult();
    }

    protected async Task ClearDb()
    {
        await Client.ClearDb();
    }
}
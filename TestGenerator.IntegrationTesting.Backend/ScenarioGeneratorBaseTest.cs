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
    
    private static Dictionary<Type, List<GeneratedResult>> AllScenarioDatasets
        = new Dictionary<Type, List<GeneratedResult>>();
    
    protected ScenarioGeneratorBaseTest(
        ITestOutputHelper testOutputHelper
    )
    {
        TestOutputHelper = testOutputHelper;
        
        var testGeneratorFixture = new TestGeneratorFixture(testOutputHelper);
        var backendFixture = new BackendFixture(testOutputHelper, GetType().Name);
        
        Client = backendFixture.BtmsClient;

        lock (typeof(T))
        {
            if (AllScenarioDatasets.TryGetValue(typeof(T), out var loadedData))
            {
                testOutputHelper.WriteLine("Scenario is cached. Using cached data");
                LoadedData = loadedData;
            }
            else
            {
                testOutputHelper.WriteLine("Scenario is not cached, loading via test generator");
                
                var data = testGeneratorFixture
                    .GenerateTestData<T>();
        
                LoadedData = backendFixture
                    .LoadTestData(data)
                    .GetAwaiter()
                    .GetResult();
                
                AllScenarioDatasets.Add(typeof(T), LoadedData);
            }
        }
        
    }

    protected async Task ClearDb()
    {
        await Client.ClearDb();
    }
}
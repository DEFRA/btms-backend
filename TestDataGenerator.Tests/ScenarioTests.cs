using Btms.BlobService;
using Btms.BlobService.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TestDataGenerator.Extensions;
using Xunit;

namespace TestDataGenerator.Tests;

public class ScenarioTests
{
    [Fact]
    public void EnsureAllScenarioDefaultsAreValid()
    {
        // var (configuration, _) = BuilderExtensions.GetConfig("Scenarios/Samples");
        //
        // var sp = new ServiceCollection()
        //     .AddBlobStorage(configuration)
        //     .AddSingleton<CachingBlobService>()
        //     .ConfigureTestGenerationServices()
        //     .BuildServiceProvider();
        
        var scenarioTypes = BuilderExtensions.GetAllScenarios();
        
        foreach (var scenarioType in scenarioTypes)
        {
            
        }
    }
}
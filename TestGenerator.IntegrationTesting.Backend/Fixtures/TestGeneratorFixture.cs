using Btms.Backend.Data;
using Btms.Backend.Data.Mongo;
using Btms.BlobService;
using Btms.Common.Extensions;
using Btms.Consumers.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog.Extensions.Logging;
using TestDataGenerator;
using TestDataGenerator.Extensions;
using TestDataGenerator.Scenarios;
using Xunit.Abstractions;

namespace TestGenerator.IntegrationTesting.Backend.Fixtures;

public class TestGeneratorFixture
{
    
    
    private IHost TestGeneratorApp { get; set; }
    
    public TestGeneratorFixture()
    {   
        // Generate test data
        
        var generatorBuilder = new HostBuilder();
        generatorBuilder
            .ConfigureTestDataGenerator("Scenarios/Samples");
        
        TestGeneratorApp = generatorBuilder.Build();
    }
    
    public List<GeneratedResult> GenerateTestData<T>() where T : ScenarioGenerator
    {   
        // TODO: Need a logger
        var logger = NullLogger.Instance;
        
        var scenarioConfig =
            TestGeneratorApp.Services.CreateScenarioConfig<T>(1, 1, arrivalDateRange: 0);
    
        var generatorResults = scenarioConfig
            .Generate(logger, 0);

        return generatorResults
            .SelectMany(r => r.Select(m => new { Result = r, Message = m}))
            .Select(rm => new GeneratedResult()
            {
                Count = 1, DateOffset = 1, Scenario = 1,
                GeneratorResult = rm.Result,
                Message = rm.Message
            })
            // .Select(r =>
            //     
            // )
            .ToList();
    }
}
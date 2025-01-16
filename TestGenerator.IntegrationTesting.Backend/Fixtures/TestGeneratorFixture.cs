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
using Serilog;
using Serilog.Extensions.Logging;
using TestDataGenerator;
using TestDataGenerator.Config;
using TestDataGenerator.Extensions;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit.Abstractions;

namespace TestGenerator.IntegrationTesting.Backend.Fixtures;

public class TestGeneratorFixture
{
    private readonly IHost _testGeneratorApp;
    private readonly ILogger<TestGeneratorFixture> Logger;
    
    public TestGeneratorFixture(ITestOutputHelper testOutputHelper)
    {   
        Logger = testOutputHelper
            .GetLogger<TestGeneratorFixture>();
        
        // Generate test data
        
        var generatorBuilder = new HostBuilder();
        generatorBuilder
            .ConfigureTestDataGenerator("Scenarios/Samples");
        
        _testGeneratorApp = generatorBuilder.Build();
    }
    
    public List<GeneratedResult> GenerateTestData<T>() where T : ScenarioGenerator
    {   
        // TODO: Need a logger
        // var logger = NullLogger.Instance;
        
        var scenarioConfig =
            _testGeneratorApp.Services.CreateScenarioConfig<T>(1, 1, arrivalDateRange: 0);
    
        var generatorResults = scenarioConfig
            .Generate(Logger, 0);

        return generatorResults
            .SelectMany(r => r.Select(m => new { Result = r, Message = m}))
            .Select(rm => new GeneratedResult()
            {
                Count = 1, DateOffset = 1, Scenario = 1,
                GeneratorResult = rm.Result,
                Message = rm.Message,
                Filepath = ""
            })
           .ToList();
    }
    
    
    public async Task<List<GeneratedResult>> GenerateTestDataset(string datasetName)
    {
        var dataset = Datasets
            .GetDatasets(_testGeneratorApp)
            .Single(d => d.Name == datasetName);
        
        var generator = _testGeneratorApp.Services
            .GetRequiredService<Generator>();
        
        var scenario = 1;
        var output = new List<GeneratedResult>();
        foreach (var s in dataset.Scenarios)
        {
            var result = await generator.Generate(scenario, s, dataset.RootPath);

            output.AddRange(result
                .SelectMany(r => r.result.Select(m => new { Result = r, Message = m }))
                .Select(rm => new GeneratedResult() {
                    Count = rm.Result.count, DateOffset = rm.Result.dateOffset,
                    Scenario = rm.Result.scenario,
                    GeneratorResult = rm.Result.result,
                    Message = rm.Message,
                    Filepath = ""
                })
            );
            
            scenario++;
        }

        Logger.LogInformation("{Dataset} Done, {MessageCount}", dataset.Name, output.Count);
    
        // var generatorResults = scenarioConfig
        //     .Generate(logger, 0);
        //
        // return generatorResults
        //     .SelectMany(r => r.Select(m => new { Result = r, Message = m}))
        //     .Select(rm => new GeneratedResult()
        //     {
        //         Count = 1, DateOffset = 1, Scenario = 1,
        //         GeneratorResult = rm.Result,
        //         Message = rm.Message
        //     })
        //     // .Select(r =>
        //     //     
        //     // )
        //     .ToList();

        return output;
    }
}
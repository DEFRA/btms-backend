using Microsoft.Extensions.Hosting;
using TestDataGenerator.Scenarios;

namespace TestDataGenerator.Config;

public class Dataset
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string RootPath { get; set; }
    public required ScenarioConfig[] Scenarios { get; set; }
}

public class Datasets(IHost app)
{   
    public static Dataset[] GetDatasets(IHost app)
    {
        var ds = new Datasets(app);
        // return ds.Sets;
        return
        [
            ds.EndToEndIbm,
            ds.One,
            ds.Basic,
            ds.LoadTest,
            ds.LoadTest90Dx1,
            ds.LoadTestCondensed,
            ds.LoadTest90Dx10k,
            ds.AllChedNoMatch
        ];
    }
    
    public readonly Dataset EndToEndIbm = new()
    {
        Name = "EndToEnd-IBM",
        Description = "WIP : A set of scenarios generated from IBMs scenario definitions",
        RootPath = "GENERATED-ENDTOEND-IBM",
        Scenarios = new[] { app.Services.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 1) }
    };

    public readonly Dataset One = new()
    {
        Name = "One",
        Description = "A dataset with a single example of each scenario",
        RootPath = "GENERATED-ONE",
        Scenarios = new[]
        {
            app.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(1, 1),
            app.Services.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 1),
            app.Services.CreateScenarioConfig<ChedPMultiStepScenarioGenerator>(1, 1)
        }
    };

    public readonly Dataset Basic = new()
    {
        Name = "Basic",
        Description = "A small functional dataset",
        RootPath = "GENERATED-BASIC",
        Scenarios = new[]
        {
            app.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(3, 7),
            app.Services.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(3, 7),
            app.Services.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(3, 7)
        }
    };

    public readonly Dataset LoadTest = new()
    {
        Name = "LoadTest",
        Description = "Generates a small load test like set to allow us to test the load test infrastructure",
        RootPath = "GENERATED-LOADTEST",
        Scenarios = new[]
        {
            app.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(100, 90),
            app.Services.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(100, 90),
            app.Services.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(100, 90)
        }
    };

    public readonly Dataset LoadTestCondensed = new()
    {
        Name = "LoadTest-Condensed",
        Description = "Generates a very small load test like set to allow us to test the load test infrastructure",
        RootPath = "GENERATED-CONDENSED",
        Scenarios = new[]
        {
            app.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(5, 7),
            app.Services.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(5, 7),
            app.Services.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(15, 7),
            app.Services.CreateScenarioConfig<CrNoMatchScenarioGenerator>(15, 7)
        }
    };

    public readonly Dataset LoadTest90Dx10k = new()
    {
        Name = "LoadTest-90Dx10k",
        Description = "'Full' Load Test - Generates significant numbers of each scenario per day over a 90d period",
        RootPath = "GENERATED-LOADTEST-90Dx10k",
        Scenarios =
            new[]
            {
                app.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(5000, 90),
                app.Services.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(100, 90),
                app.Services.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(4900, 90)
            }
    };

    public readonly Dataset LoadTest90Dx1 = new()
    {
        Name = "90Dx1",
        Description = "Generates a single example of each scenario per day over a 90d period",
        RootPath = "GENERATED-90Dx1",
        Scenarios =
            new[]
            {
                app.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(1, 90),
                app.Services.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(1, 90),
                app.Services.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 90)
            }
    };

    public readonly Dataset AllChedNoMatch = new()
    {
        Name = "All-CHED-No-Match",
        Description = "LIM TODO",
        RootPath = "GENERATED-ALL-CHED-NO-MATCH",
        Scenarios = new[] { app.Services.CreateScenarioConfig<AllChedsNoMatchScenarioGenerator>(1, 1) }
    };

    // public readonly Dataset Pha = new()
    // {
    //     Name = "PHA",
    //     Description = "",
    //     RootPath = "GENERATED-PHA",
    //     Scenarios = new[]
    //     {
    //         app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 30),
    //         app.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(10, 30)
    //     }
    // };
}
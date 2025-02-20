using Microsoft.Extensions.Hosting;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.SpecificFiles;
using AllChedsNoMatchScenarioGenerator = TestDataGenerator.Scenarios.SpecificFiles.AllChedsNoMatchScenarioGenerator;

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
    public const string FunctionalAnalyticsDatasetName = "Functional-Analytics";
    public const string FunctionalAnalyticsDecisionsDatasetName = "Functional-Analytics-Decisions";
    public const string FunctionalAnalyticsDecisionStatusDatasetName = "Functional-Analytics-DecisionStatus";

    public static Dataset[] GetDatasets(IHost app)
    {
        var ds = new Datasets(app);

        return
        [
            ds.EndToEndIbm,
            ds.One,
            ds.Basic,
            ds.LoadTest,
            ds.LoadTest90Dx1,
            ds.LoadTestCondensed,
            ds.LoadTest90Dx10k,
            ds.FunctionalAnalytics,
            ds.FunctionalAnalyticsDecisions,
            ds.FunctionalAnalyticsDecisionStatus
        ];
    }

    public readonly Dataset FunctionalAnalytics = new()
    {
        Name = FunctionalAnalyticsDatasetName,
        Description = "Functional Testing Analytics Dataset",
        RootPath = "FUNCTIONAL-ANALYTICS",
        Scenarios = new[]
        {
            // Migrates setup from BasicSampleDataTestFixture in analytics testing
            
            // Ensure we have some data scenarios around 24/48 hour tests
            
            app.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(2, 2, arrivalDateRange: 0),
            app.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchScenarioGenerator>(2, 2, arrivalDateRange: 2),
            app.Services.CreateScenarioConfig<CrNoMatchScenarioGenerator>(2, 2, arrivalDateRange: 0),
            
            // Create some more variable data over the rest of time

            app.Services.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(2, 2, arrivalDateRange: 10),
            app.Services.CreateScenarioConfig<ChedANoMatchScenarioGenerator>(2, 2, arrivalDateRange: 10),
            app.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchScenarioGenerator>(1, 2, arrivalDateRange: 10),
            app.Services.CreateScenarioConfig<CrNoMatchScenarioGenerator>(1, 2, arrivalDateRange: 10)
        }
    };

    public readonly Dataset FunctionalAnalyticsDecisions = new()
    {
        Name = FunctionalAnalyticsDecisionsDatasetName,
        Description = "Functional Testing Analytics Dataset for testing decision analytics",
        RootPath = "FUNCTIONAL-ANALYTICS-DECISIONS",
        Scenarios = new[]
        {
            app.Services.CreateScenarioConfig<CrNoMatchSingleItemWithDecisionScenarioGenerator>(2, 2, arrivalDateRange: 0),
            app.Services.CreateScenarioConfig<CrNoMatchNoDecisionScenarioGenerator>(2, 2, arrivalDateRange: 2),
        }
    };


    public readonly Dataset FunctionalAnalyticsDecisionStatus = new()
    {
        Name = FunctionalAnalyticsDecisionStatusDatasetName,
        Description = "Functional Testing Analytics Dataset for testing business decision status chart",
        RootPath = "FUNCTIONAL-ANALYTICS-DECISION-STATUS",
        Scenarios = new[]
        {
            // Same BTMS & ALVS Decision
            app.Services.CreateScenarioConfig<Mrn24Gbddjer3Zfrmzar9ScenarioGenerator>(1, 1, arrivalDateRange: 0),
            
            // Destroyed
            app.Services.CreateScenarioConfig<Mrn24Gbdej9V2Od0Bhar0DestroyedScenarioGenerator>(1, 1, arrivalDateRange: 0),
            
            // Manual Action
            app.Services.CreateScenarioConfig<Mrn24Gbeds4W7Dfrlmar0ManualActionScenarioGenerator>(1, 1, arrivalDateRange: 0),
            
            // Cleared
            app.Services.CreateScenarioConfig<Mrn24Gbd2Uowtwym5Lar8ScenarioGenerator>(1, 1, arrivalDateRange: 0),
            
            // No Finalisation
            app.Services.CreateScenarioConfig<Mrn24Gbdshixsy6Rckar3ScenarioGenerator>(1, 1, arrivalDateRange: 0),
        }
    };

    public readonly Dataset EndToEndIbm = new()
    {
        Name = "EndToEnd-IBM",
        Description = "WIP : A set of scenarios generated from IBMs scenario definitions",
        RootPath = "GENERATED-ENDTOEND-IBM",
        Scenarios = new[]
        {
            app.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchScenarioGenerator>(1,
                1)
        }
    };

    public readonly Dataset One = new()
    {
        Name = "One",
        Description = "A dataset with a single example of each scenario",
        RootPath = "GENERATED-ONE",
        Scenarios = new[]
        {
            app.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchScenarioGenerator>(1,
                1),
            app.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchScenarioGenerator>(1,
                1),
            app.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.MultiStepScenarioGenerator>(1,
                1)
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
            app.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchScenarioGenerator>(3,
                7)
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
            app.Services.CreateScenarioConfig<TestDataGenerator.Scenarios.ChedP.SimpleMatchScenarioGenerator>(
                100, 90)
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
            app.Services.CreateScenarioConfig<Scenarios.ChedP.SimpleMatchScenarioGenerator>(15, 7),
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
                app.Services.CreateScenarioConfig<Scenarios.ChedP.SimpleMatchScenarioGenerator>(4900, 90)
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
                app.Services.CreateScenarioConfig<Scenarios.ChedP.SimpleMatchScenarioGenerator>(1, 90)
            }
    };
}
using TestDataGenerator;

namespace TestGenerator.IntegrationTesting.Backend;

public class GeneratedResult
{
    public required ScenarioGenerator.GeneratorResult GeneratorResult { get; set; }
    public required int Scenario { get; set; }
    public required int DateOffset { get; set; }
    public required int Count { get; set; }
    public required object Message { get; set; }
    
    public required string Filepath { get; set; }
}
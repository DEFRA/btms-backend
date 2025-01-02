using TestDataGenerator;

namespace TestGenerator.IntegrationTesting.Backend.Fixtures;

public class BackendGeneratorFixture<T>(
    TestGeneratorFixture testGenerator, 
    BackendFixture backend
)
    where T : ScenarioGenerator
{
    public TestGeneratorFixture TestGenerator { get; set; } = testGenerator;
    public BackendFixture Backend { get; set; } = backend;
}
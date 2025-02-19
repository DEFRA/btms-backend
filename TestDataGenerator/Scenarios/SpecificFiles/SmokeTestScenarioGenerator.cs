using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class SmokeTestScenarioGenerator(IServiceProvider sp, ILogger<SmokeTestScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger, "SmokeTest");
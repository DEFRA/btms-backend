using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class LinkingScenarioGenerator(IServiceProvider sp, ILogger<LinkingScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger, "Linking");
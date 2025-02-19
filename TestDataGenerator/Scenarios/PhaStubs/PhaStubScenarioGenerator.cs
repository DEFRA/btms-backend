using Microsoft.Extensions.Logging;
using TestDataGenerator.Scenarios.SpecificFiles;

namespace TestDataGenerator.Scenarios.PhaStubs;


public class PhaStubScenarioGenerator(IServiceProvider sp, ILogger<PhaStubScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "PhaStub")
{

}

public class PhaFinalisationStubScenarioGenerator(IServiceProvider sp, ILogger<PhaStubScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger, "PhaFinalisationStub")
{

}


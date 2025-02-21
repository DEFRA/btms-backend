using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class Mrn24Gbdej9V2Od0Bhar0DestroyedScenarioGenerator(IServiceProvider sp, ILogger<Mrn24Gbdej9V2Od0Bhar0DestroyedScenarioGenerator> logger) :
    SpecificFilesScenarioGenerator(sp, logger, "Mrn-24GBDEJ9V2OD0BHAR0")
{
    protected override List<(string filePath, IBaseBuilder builder)> ModifyBuilders(List<(string filePath, IBaseBuilder builder)> builders, int? scenario = null, int? item = null, DateTime? entryDate = null)
    {
        builders = base.ModifyBuilders(builders, scenario, item, entryDate);

        foreach (var builderItem in builders
            .Where(b => b.builder is FinalisationBuilder))
        {
            ((FinalisationBuilder)builderItem.builder)
                .WithFinalState(3);
        }

        return builders;
    }
}
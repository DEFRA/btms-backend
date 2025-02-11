using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class NoAuditLogForMovementUpdate(IServiceProvider sp, ILogger<NoAuditLogForMovementUpdate> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders = GetBuilders("NoAuditLogForMovementUpdate").GetAwaiter().GetResult();

        logger.LogInformation("Created {builders} Builders",
            builders.Count);

        var updatedCr = ((ClearanceRequestBuilder)builders
                .Single(b => b.builder is ClearanceRequestBuilder)
                .builder)
            .Clone()
            .IncrementCreationDate(TimeSpan.FromHours(2))
            .IncrementEntryVersionNumber()
            .WithItemDocumentRef("GBCHD2024.001239999999", 1, 0)
            .ValidateAndBuild();

        var messages = builders
            .Select(b => b.builder)
            .ToArray()
            .BuildAll()
            .Concat([updatedCr])
            .ToArray();

        return new GeneratorResult(messages);
    }
}
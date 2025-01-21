using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class Mrn24GBDE3CF94H96TAR0ModifiedScenarioGenerator(IServiceProvider sp, ILogger<Mrn24GBDE3CF94H96TAR0ScenarioGenerator> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders =  GetBuilders("Mrn-24GBDE3CF94H96TAR0").GetAwaiter().GetResult();
        
        logger.LogInformation("Created {builders} Builders", 
            builders.Count);

        var decisionV1Builder =
            (DecisionBuilder)builders.Single(b => b.filePath == Path.Join("Mrn-24GBDE3CF94H96TAR0", "DECISIONS", "2024", "12", "03", "24GBDE3CF94H96TAR0-0deb3808-c55e-4196-9270-50ff1ee2ce49.json")).builder;

        decisionV1Builder.Do(d =>
        {
            // In the raw files, v2 arrives 2 mins after v1, reverse them
            d.ServiceHeader!.ServiceCallTimestamp = d.ServiceHeader.ServiceCallTimestamp!.Value.AddMinutes(15);
        });
        
        var messages = builders
            .Select(b => b.builder)
            .ToArray()
            .BuildAll()
            .ToArray();
        
        logger.LogInformation("Created times of clearance requests are {Created}", messages
            .OfType<AlvsClearanceRequest>()
            .Select(d => (d.Header!.EntryVersionNumber, d.ServiceHeader!.ServiceCallTimestamp))
            .ToArray()
        );
        
        logger.LogInformation("Created times of decisions are {Created}", messages
            .OfType<Decision>()
            .Select(d => (d.Header!.EntryVersionNumber, d.Header!.DecisionNumber, d.ServiceHeader!.ServiceCallTimestamp))
            .ToArray()
        );
        
        return new GeneratorResult(messages);
    }
}
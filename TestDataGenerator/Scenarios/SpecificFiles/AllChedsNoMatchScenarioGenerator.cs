using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class AllChedsNoMatchScenarioGenerator(IServiceProvider sp, ILogger<AllChedsNoMatchScenarioGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders = GetBuilders("AllChedsNoMatch").GetAwaiter().GetResult();

        logger.LogInformation("Created {builders} Builders",
            builders.Count);

       
        var chedAMessage = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Combine("AllChedsNoMatch", "IPAFFS", "2024", "11", "01", "cheda-one-commodity.json"))
                .builder).WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
            .ValidateAndBuild();

        var chedPMessage = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Combine("AllChedsNoMatch", "IPAFFS", "2024", "11", "01", "chedp-one-commodity.json"))
                .builder)
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, item)
            .ValidateAndBuild();

        var chedDMessage = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Combine("AllChedsNoMatch", "IPAFFS", "2024", "11", "01", "chedd-one-commodity.json"))
                .builder)
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Ced, scenario, entryDate, item)
            .ValidateAndBuild();
        
        var chedPPMessage = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Combine("AllChedsNoMatch", "IPAFFS", "2024", "11", "01", "chedpp-multiple-commodity.json"))
                .builder)
        .WithCreationDate(entryDate)
        .WithRandomArrivalDateTime(config.ArrivalDateRange)
        .WithReferenceNumber(ImportNotificationTypeEnum.Chedpp, scenario, entryDate, item)
        .ValidateAndBuild();


        return new GeneratorResult([chedAMessage, chedPMessage, chedDMessage, chedPPMessage]);
    }
}
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios;

public class ChedANoMatchScenarioGenerator(ILogger<ChedANoMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = GetNotificationBuilder("cheda-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
            .ValidateAndBuild();

        logger.LogInformation("Created {NotificationReferenceNumber}", 
            notification.ReferenceNumber);

        return new GeneratorResult([notification]);
    }
}
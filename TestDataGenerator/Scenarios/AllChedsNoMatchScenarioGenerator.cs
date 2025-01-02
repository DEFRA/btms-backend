using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios;

public class AllChedsNoMatchScenarioGenerator(ILogger<AllChedsNoMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var chedANotification = GetNotificationBuilder("cheda-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
            .WithVersionNumber()
            .ValidateAndBuild();

        logger.LogInformation("Created {NotificationReferenceNumber}", 
            chedANotification.ReferenceNumber);
        
        var chedPNotification = GetNotificationBuilder("chedp-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, item)
            .WithVersionNumber()
            .ValidateAndBuild();
        
        logger.LogInformation("Created {NotificationReferenceNumber}", 
            chedPNotification.ReferenceNumber);
        
        var chedDNotification = GetNotificationBuilder("chedd-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Ced, scenario, entryDate, item)
            .WithVersionNumber()
            .ValidateAndBuild();
        
        logger.LogInformation("Created {NotificationReferenceNumber}", 
            chedDNotification.ReferenceNumber);
        
        var chedPPNotification = GetNotificationBuilder("chedpp-multiple-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Chedpp, scenario, entryDate, item)
            .WithVersionNumber()
            .ValidateAndBuild();
        
        logger.LogInformation("Created {NotificationReferenceNumber}", 
            chedPPNotification.ReferenceNumber);

        return new GeneratorResult([chedANotification, chedPNotification, chedDNotification, chedPPNotification]);
    }
}
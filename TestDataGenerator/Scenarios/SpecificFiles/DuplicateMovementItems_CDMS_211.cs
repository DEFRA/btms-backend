using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class DuplicateMovementItems_CDMS_211(IServiceProvider sp, ILogger<DuplicateMovementItems_CDMS_211> logger) : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders =  GetBuilders("DuplicateMovementItems-CDMS-211").GetAwaiter().GetResult();
        
        logger.LogInformation("Created {NotificationReferenceNumber} Builders", 
            builders.Count);

        var messages = builders
            .Select(b => b.builder)
            .ToArray()
            .BuildAll();
        
        return new GeneratorResult(messages);
        
        // var notification = GetNotificationBuilder("chedp-one-commodity")
        //     .WithCreationDate(entryDate)
        //     .WithRandomArrivalDateTime(config.ArrivalDateRange)
        //     .ValidateAndBuild();
        //
        // logger.LogInformation("Created {NotificationReferenceNumber}", 
        //     notification.ReferenceNumber);
        //
        // // TODO - check with Matt what a sensible checkCode, decision & other fields we need to 
        // // implement a 'real world' test here
        // var checkCode = "H2019";
        // var decisionCode = "H01";

        // var clearanceRequestV1 = GetClearanceRequestBuilder("DuplicateMovementItems-CDMS-211/ALVS/2024/11/01/24GBC4EB0D97OK4AR4-d728e21f-a12d-4d8f-aa49-f73b7c2f1065")
        //     // .WithCreationDate(entryDate.AddHours(2), false)
        //     // .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
        //     // .WithReferenceNumber(notification.ReferenceNumber!)
        //     .ValidateAndBuild();
        //
        // logger.LogInformation("Created {EntryReference}", clearanceRequestV1.Header!.EntryReference);
        //
        //
        // var clearanceRequestV2 = GetClearanceRequestBuilder("DuplicateMovementItems-CDMS-211/ALVS/2024/11/08/24GBC4EB0D97OK4AR4-2ad7665a-0020-4e9f-8d3a-d2cfa830e4c1")
        //     // .WithCreationDate(entryDate.AddHours(2), false)
        //     // .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
        //     // .WithReferenceNumber(notification.ReferenceNumber!)
        //     .ValidateAndBuild();
        //
        // logger.LogInformation("Created {EntryReference}", clearanceRequestV2.Header!.EntryReference);
        //
        // // var alvsDecision = GetDecisionBuilder("decision-one-item")
        // //     .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(1), false)
        // //     .WithReferenceNumber(notification.ReferenceNumber!)
        // //     .WithEntryVersionNumber(1)
        // //     .WithDecisionVersionNumber()
        // //     .WithItemAndCheck(1, checkCode, decisionCode)
        // //     .ValidateAndBuild();
        //
        // // logger.LogInformation("Created {EntryReference}", alvsDecision.Header!.EntryReference);
        //
        // return new GeneratorResult([clearanceRequestV1, clearanceRequestV2]);
    }
}
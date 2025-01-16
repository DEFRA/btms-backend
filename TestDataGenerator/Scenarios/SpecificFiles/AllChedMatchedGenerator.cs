using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class AllChedsH02DecisionGenerator(IServiceProvider sp, ILogger<AllChedsH02DecisionGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders = GetBuilders("AllChedsWithDecision").GetAwaiter().GetResult();

        logger.LogInformation("Created {builders} Builders",
            builders.Count);

        var random = new Random();
        var chedAImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "IPAFFS", "cheda.json"))
                .builder)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, random.Next(1, 100))
            .WithInspectionStatus()
            .WithImportNotificationStatus()
            .ValidateAndBuild();

        var chedPImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "IPAFFS", "chedp.json"))
                .builder)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, random.Next(1, 100))
            .ValidateAndBuild();

        var chedDImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "IPAFFS", "chedd.json"))
                .builder)
            .WithReferenceNumber(ImportNotificationTypeEnum.Ced, scenario, entryDate, random.Next(1, 100))
            .WithInspectionStatus()
            .WithImportNotificationStatus()
            .ValidateAndBuild();

        var chedPPImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "IPAFFS", "chedpp.json"))
                .builder)
            .WithReferenceNumber(ImportNotificationTypeEnum.Chedpp, scenario, entryDate, random.Next(1, 100))
            .ValidateAndBuild();

        var chedAClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "ALVS", "cr-cheda-match.json"))
                .builder)
            .WithReferenceNumberOneToOne(chedAImportNotification.ReferenceNumber!)
            .ValidateAndBuild();

        var chedPClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "ALVS", "cr-chedp-match.json"))
                .builder)
            .WithReferenceNumberOneToOne(chedPImportNotification.ReferenceNumber!)
            .ValidateAndBuild();

        var chedDClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "ALVS", "cr-chedd-match.json"))
                .builder)
            .WithReferenceNumberOneToOne(chedDImportNotification.ReferenceNumber!)
            .ValidateAndBuild();

        var chedPPClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "ALVS", "cr-chedpp-match.json"))
                .builder)
            .WithReferenceNumberOneToOne(chedPPImportNotification.ReferenceNumber!)
            .ValidateAndBuild();


        return new GeneratorResult([
            chedAClearanceRequest, chedDClearanceRequest, chedPClearanceRequest, chedPPClearanceRequest,
            chedAImportNotification, chedDImportNotification, chedPImportNotification, chedPPImportNotification
        ]);
    }
}

public class AllChedsNonHoldDecisionGenerator(IServiceProvider sp, ILogger<AllChedsH02DecisionGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders = GetBuilders("AllChedsWithDecision").GetAwaiter().GetResult();

        logger.LogInformation("Created {builders} Builders",
            builders.Count);

        var random = new Random();
        var decision = new Decision();
        decision.ConsignmentAcceptable = true;
        decision.DecisionEnum = DecisionDecisionEnum.AcceptableForInternalMarket;
        
        var chedAImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "IPAFFS", "cheda.json"))
                .builder)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, random.Next(1, 100))
            .WithImportNotificationStatus(ImportNotificationStatusEnum.Validated)
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();

        var chedPImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "IPAFFS", "chedp.json"))
                .builder)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, random.Next(1, 100))
            .WithImportNotificationStatus(ImportNotificationStatusEnum.Validated)
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();

        var chedDImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "IPAFFS", "chedd.json"))
                .builder)
            .WithReferenceNumber(ImportNotificationTypeEnum.Ced, scenario, entryDate, random.Next(1, 100))
            .WithImportNotificationStatus(ImportNotificationStatusEnum.Validated)
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();

        var chedPPImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "IPAFFS", "chedpp.json"))
                .builder)
            .WithReferenceNumber(ImportNotificationTypeEnum.Chedpp, scenario, entryDate, random.Next(1, 100))
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();

        var chedAClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "ALVS", "cr-cheda-match.json"))
                .builder)
            .WithReferenceNumberOneToOne(chedAImportNotification.ReferenceNumber!)
            .ValidateAndBuild();

        var chedPClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "ALVS", "cr-chedp-match.json"))
                .builder)
            .WithReferenceNumberOneToOne(chedPImportNotification.ReferenceNumber!)
            .ValidateAndBuild();

        var chedDClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "ALVS", "cr-chedd-match.json"))
                .builder)
            .WithReferenceNumberOneToOne(chedDImportNotification.ReferenceNumber!)
            .ValidateAndBuild();

        var chedPPClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join("AllChedsWithDecision", "ALVS", "cr-chedpp-match.json"))
                .builder)
            .WithReferenceNumberOneToOne(chedPPImportNotification.ReferenceNumber!)
            .ValidateAndBuild();


        return new GeneratorResult([
            chedAClearanceRequest, chedDClearanceRequest, chedPClearanceRequest, chedPPClearanceRequest,
            chedAImportNotification, chedDImportNotification, chedPImportNotification, chedPPImportNotification
        ]);
    }
}
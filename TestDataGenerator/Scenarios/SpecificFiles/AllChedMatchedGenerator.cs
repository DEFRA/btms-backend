using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class ChedAh01ScenarioScenarioGenerator()
    : H01ScenarioGenerator("cheda", inspectionRequired: InspectionRequiredEnum.NotRequired) { }

public class ChedPh01ScenarioScenarioGenerator()
    : H01ScenarioGenerator("chedp", includeImportNotificationStatus: false) { }

public class ChedDh01ScenarioScenarioGenerator()
    : H01ScenarioGenerator("chedd") { }

public abstract class H01ScenarioGenerator(string filename, InspectionRequiredEnum inspectionRequired = InspectionRequiredEnum.Inconclusive, bool includeImportNotificationStatus = true)
    : ScenarioGenerator
{
    private const string AllChedsWithDecisionScenarioPath = "AllChedsWithDecision";

    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var importNotification = GetNotificationBuilder($"{AllChedsWithDecisionScenarioPath}/IPAFFS/{filename}")
            .WithInspectionStatus(inspectionRequired)
            .WithOptionalStep(includeImportNotificationStatus, 
                a => a.WithImportNotificationStatus())
            // .WithImportNotificationStatus()
            .ValidateAndBuild();    
        
        var clearanceRequest = GetClearanceRequestBuilder($"{AllChedsWithDecisionScenarioPath}/ALVS/cr-{filename}-match") 
            .ValidateAndBuild();
        
        return new GeneratorResult([
            clearanceRequest, 
            importNotification, 
        ]);
    }
}

public class AllChedsH01DecisionGenerator(IServiceProvider sp, ILogger<AllChedsH01DecisionGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger)
{
    private const string AllChedsWithDecisionScenarioPath = "AllChedsWithDecision";

    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders = GetBuilders(AllChedsWithDecisionScenarioPath).GetAwaiter().GetResult();

        logger.LogInformation("Created {builders} Builders",
            builders.Count);
        
        var random = new Random();
        var chedAImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "IPAFFS", "cheda.json"))
                .builder)
            .WithInspectionStatus(InspectionRequiredEnum.NotRequired)
            .WithImportNotificationStatus()
            .ValidateAndBuild();

        var chedPImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "IPAFFS", "chedp.json"))
                .builder)
            .WithInspectionStatus(InspectionRequiredEnum.Inconclusive)
            .ValidateAndBuild();

        var chedDImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "IPAFFS", "chedd.json"))
                .builder)
            .WithInspectionStatus(InspectionRequiredEnum.Inconclusive)
            .WithImportNotificationStatus()
            .ValidateAndBuild();


        var chedAClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "ALVS", "cr-cheda-match.json"))
                .builder)
            .ValidateAndBuild();

        var chedPClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "ALVS", "cr-chedp-match.json"))
                .builder)
            .ValidateAndBuild();

        var chedDClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "ALVS", "cr-chedd-match.json"))
                .builder)
            .ValidateAndBuild();


        return new GeneratorResult([
            chedAClearanceRequest, chedDClearanceRequest, chedPClearanceRequest, 
            chedAImportNotification, chedDImportNotification, chedPImportNotification, 
        ]);
    }
}

public class ChedAh02ScenarioScenarioGenerator()
    : H02ScenarioGenerator("cheda") { }

public class ChedPh02ScenarioScenarioGenerator()
    : H02ScenarioGenerator("chedp", includeImportNotificationStatusAndInspectionStatus : false) { }

public class ChedDh02ScenarioScenarioGenerator()
    : H02ScenarioGenerator("chedd") { }

public abstract class H02ScenarioGenerator(string filename, bool includeImportNotificationStatusAndInspectionStatus = true)
    : ScenarioGenerator
{
    private const string AllChedsWithDecisionScenarioPath = "AllChedsWithDecision";

    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var importNotification = GetNotificationBuilder($"{AllChedsWithDecisionScenarioPath}/IPAFFS/{filename}")
            .WithOptionalStep(includeImportNotificationStatusAndInspectionStatus, 
                a => a
                    .WithInspectionStatus()
                    .WithImportNotificationStatus()
                )
            .ValidateAndBuild();    
        
        var clearanceRequest = GetClearanceRequestBuilder($"{AllChedsWithDecisionScenarioPath}/ALVS/cr-{filename}-match") 
            .ValidateAndBuild();
        
        return new GeneratorResult([
            clearanceRequest, 
            importNotification, 
        ]);
    }
}

public class AllChedsH02DecisionGenerator(IServiceProvider sp, ILogger<AllChedsH02DecisionGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger)
{
    private const string AllChedsWithDecisionScenarioPath = "AllChedsWithDecision";

    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders = GetBuilders(AllChedsWithDecisionScenarioPath).GetAwaiter().GetResult();

        logger.LogInformation("Created {builders} Builders",
            builders.Count);

        var random = new Random();
        var chedAImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "IPAFFS", "cheda.json"))
                .builder)
            .WithInspectionStatus()
            .WithImportNotificationStatus()
            .ValidateAndBuild();

        var chedPImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "IPAFFS", "chedp.json"))
                .builder)
            .ValidateAndBuild();

        var chedDImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "IPAFFS", "chedd.json"))
                .builder)
            .WithInspectionStatus()
            .WithImportNotificationStatus()
            .ValidateAndBuild();

        var chedAClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "ALVS", "cr-cheda-match.json"))
                .builder)
            .ValidateAndBuild();

        var chedPClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "ALVS", "cr-chedp-match.json"))
                .builder)
            .ValidateAndBuild();

        var chedDClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "ALVS", "cr-chedd-match.json"))
                .builder)
            .ValidateAndBuild();


        return new GeneratorResult([
            chedAClearanceRequest, chedDClearanceRequest, chedPClearanceRequest, 
            chedAImportNotification, chedDImportNotification, chedPImportNotification, 
        ]);
    }
}

public class ChedAc03ScenarioScenarioGenerator()
    : C03ScenarioGenerator("cheda") { }

public class ChedPc03ScenarioScenarioGenerator()
    : C03ScenarioGenerator("chedp") { }

public class ChedDc03ScenarioScenarioGenerator()
    : C03ScenarioGenerator("chedd") { }

public abstract class C03ScenarioGenerator(string filename)
    : ScenarioGenerator
{
    private const string AllChedsWithDecisionScenarioPath = "AllChedsWithDecision";

    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var random = new Random();
        var decision = new Decision();
        decision.ConsignmentAcceptable = true;
        decision.DecisionEnum = DecisionDecisionEnum.AcceptableForInternalMarket;
        
        var importNotification = GetNotificationBuilder($"{AllChedsWithDecisionScenarioPath}/IPAFFS/{filename}")
            .WithImportNotificationStatus(ImportNotificationStatusEnum.Validated)
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();    
        
        var clearanceRequest = GetClearanceRequestBuilder($"{AllChedsWithDecisionScenarioPath}/ALVS/cr-{filename}-match") 
            .ValidateAndBuild();
        
        return new GeneratorResult([
            clearanceRequest, 
            importNotification, 
        ]);
    }
}

public class AllChedsC03DecisionGenerator(IServiceProvider sp, ILogger<AllChedsH02DecisionGenerator> logger)
    : SpecificFilesScenarioGenerator(sp, logger)
{
    private const string AllChedsWithDecisionScenarioPath = "AllChedsWithDecision";

    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var builders = GetBuilders(AllChedsWithDecisionScenarioPath).GetAwaiter().GetResult();

        logger.LogInformation("Created {builders} Builders",
            builders.Count);

        var random = new Random();
        var decision = new Decision();
        decision.ConsignmentAcceptable = true;
        decision.DecisionEnum = DecisionDecisionEnum.AcceptableForInternalMarket;
        
        var chedAImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "IPAFFS", "cheda.json"))
                .builder)
            .WithImportNotificationStatus(ImportNotificationStatusEnum.Validated)
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();

        var chedPImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "IPAFFS", "chedp.json"))
                .builder)
            .WithImportNotificationStatus(ImportNotificationStatusEnum.Validated)
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();

        var chedDImportNotification = ((ImportNotificationBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "IPAFFS", "chedd.json"))
                .builder)
            .WithImportNotificationStatus(ImportNotificationStatusEnum.Validated)
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();

        var chedAClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "ALVS", "cr-cheda-match.json"))
                .builder)
            .ValidateAndBuild();

        var chedPClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "ALVS", "cr-chedp-match.json"))
                .builder)
            .ValidateAndBuild();

        var chedDClearanceRequest = ((ClearanceRequestBuilder)builders
                .Single(b => b.filePath == Path.Join(AllChedsWithDecisionScenarioPath, "ALVS", "cr-chedd-match.json"))
                .builder)
            .ValidateAndBuild();

        return new GeneratorResult([
            chedAClearanceRequest, chedDClearanceRequest, chedPClearanceRequest,
            chedAImportNotification, chedDImportNotification, chedPImportNotification
        ]);
    }
}
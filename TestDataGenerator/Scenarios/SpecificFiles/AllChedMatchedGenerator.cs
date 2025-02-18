using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class ChedAh01ScenarioGenerator1(ILogger<ChedAh01ScenarioGenerator1> logger)
    : HoldScenarioGenerator(logger, "cheda", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.Submitted)
{ }

public class ChedAh01ScenarioGenerator2(ILogger<ChedAh01ScenarioGenerator2> logger)
    : HoldScenarioGenerator(logger, "cheda", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.Submitted)
{ }

public class ChedAh01ScenarioGenerator3(ILogger<ChedAh01ScenarioGenerator3> logger)
    : HoldScenarioGenerator(logger, "cheda", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.InProgress)
{ }

public class ChedAh01ScenarioGenerator4(ILogger<ChedAh01ScenarioGenerator4> logger)
    : HoldScenarioGenerator(logger, "cheda", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.InProgress)
{ }

public class ChedPh01ScenarioGenerator1(ILogger<ChedPh01ScenarioGenerator1> logger)
    : HoldScenarioGenerator(logger, "chedp", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.Submitted)
{ }

public class ChedPh01ScenarioGenerator2(ILogger<ChedPh01ScenarioGenerator2> logger)
    : HoldScenarioGenerator(logger, "chedp", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.Submitted)
{ }

public class ChedPh01ScenarioGenerator3(ILogger<ChedPh01ScenarioGenerator3> logger)
    : HoldScenarioGenerator(logger, "chedp", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.InProgress)
{ }

public class ChedPh01ScenarioGenerator4(ILogger<ChedPh01ScenarioGenerator4> logger)
    : HoldScenarioGenerator(logger, "chedp", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.InProgress)
{ }

public class ChedDh01ScenarioGenerator1(ILogger<ChedDh01ScenarioGenerator1> logger)
    : HoldScenarioGenerator(logger, "chedd", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.Submitted)
{ }

public class ChedDh01ScenarioGenerator2(ILogger<ChedDh01ScenarioGenerator2> logger)
    : HoldScenarioGenerator(logger, "chedd", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.Submitted)
{ }

public class ChedDh01ScenarioGenerator3(ILogger<ChedDh01ScenarioGenerator3> logger)
    : HoldScenarioGenerator(logger, "chedd", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.InProgress)
{ }

public class ChedDh01ScenarioGenerator4(ILogger<ChedDh01ScenarioGenerator4> logger)
    : HoldScenarioGenerator(logger, "chedd", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.InProgress)
{ }

public class ChedAh02ScenarioGenerator1(ILogger<ChedAh02ScenarioGenerator1> logger)
    : HoldScenarioGenerator(logger, "cheda", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.Submitted)
{ }

public class ChedAh02ScenarioGenerator2(ILogger<ChedAh02ScenarioGenerator2> logger)
    : HoldScenarioGenerator(logger, "cheda", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.InProgress)
{ }

public class ChedDh02ScenarioGenerator1(ILogger<ChedDh02ScenarioGenerator1> logger)
    : HoldScenarioGenerator(logger, "chedd", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.Submitted)
{ }

public class ChedDh02ScenarioGenerator2(ILogger<ChedDh02ScenarioGenerator2> logger)
    : HoldScenarioGenerator(logger, "chedd", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.InProgress)
{ }

public class ChedPh02ScenarioGenerator1(ILogger<ChedPh02ScenarioGenerator1> logger)
    : HoldScenarioGenerator(logger, "chedp", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.Submitted)
{ }

public class ChedPh02ScenarioGenerator2(ILogger<ChedPh02ScenarioGenerator2> logger)
    : HoldScenarioGenerator(logger, "chedp", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.InProgress)
{ }

public abstract class HoldScenarioGenerator(ILogger logger, string filename, InspectionRequiredEnum inspectionRequired, ImportNotificationStatusEnum importNotificationStatus)
    : ScenarioGenerator(logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var importNotification = BuilderHelpers.GetNotificationBuilder($"AllChedsWithDecision/IPAFFS/{filename}")
            .WithInspectionStatus(inspectionRequired)
            .WithImportNotificationStatus(importNotificationStatus)
            .ValidateAndBuild();

        var clearanceRequest = BuilderHelpers.GetClearanceRequestBuilder($"AllChedsWithDecision/ALVS/cr-{filename}-match")
            .ValidateAndBuild();

        return new GeneratorResult([
            clearanceRequest,
            importNotification,
        ]);
    }
}

public class ChedAc03ScenarioGenerator(ILogger<ChedAc03ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "cheda", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableForInternalMarket })
{ }

public class ChedPc03ScenarioGenerator(ILogger<ChedPc03ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "chedp", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableForInternalMarket })
{ }

public class ChedDc03ScenarioGenerator(ILogger<ChedDc03ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "chedd", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableForInternalMarket })
{ }

public class ChedAc05ScenarioGenerator(ILogger<ChedAc05ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "cheda", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableForTemporaryImport })
{ }

public class ChedAc06ScenarioGenerator(ILogger<ChedAc06ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "cheda", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.HorseReEntry })
{ }

public class ChedPc06ScenarioGenerator(ILogger<ChedPc06ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "chedp", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableIfChanneled })
{ }

public class ChedAn02ScenarioGenerator(ILogger<ChedAn02ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "cheda", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Euthanasia })
{ }

public class ChedAn04ScenarioGenerator(ILogger<ChedAn04ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "cheda", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Reexport })
{ }

public class ChedDn02ScenarioGenerator(ILogger<ChedDn02ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "chedd", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Destruction })
{ }

public class ChedDn04ScenarioGenerator(ILogger<ChedDn04ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "chedd", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Redispatching })
{ }

public class ChedPn02ScenarioGenerator(ILogger<ChedPn02ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "chedp", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Destruction })
{ }

public class ChedPn03ScenarioGenerator(ILogger<ChedPn03ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "chedp", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Transformation })
{ }

public class ChedPn04ScenarioGenerator(ILogger<ChedPn04ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "chedp", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Reexport })
{ }

public class ChedPn07ScenarioGenerator(ILogger<ChedPn04ScenarioGenerator> logger)
    : ReleaseAndRefusalScenarioGenerator(logger, "chedp", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Other })
{ }

public abstract class ReleaseAndRefusalScenarioGenerator(ILogger logger, string filename, Decision decision)
    : ScenarioGenerator(logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var importNotification = BuilderHelpers.GetNotificationBuilder($"AllChedsWithDecision/IPAFFS/{filename}")
            .WithImportNotificationStatus(ImportNotificationStatusEnum.Validated)
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();

        var clearanceRequest = BuilderHelpers.GetClearanceRequestBuilder($"AllChedsWithDecision/ALVS/cr-{filename}-match")
            .ValidateAndBuild();

        return new GeneratorResult([
            clearanceRequest,
            importNotification,
        ]);
    }
}
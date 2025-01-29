using Btms.Types.Ipaffs;

namespace TestDataGenerator.Scenarios.SpecificFiles;

public class ChedAh01ScenarioGenerator1()
    : HoldScenarioGenerator("cheda", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.Submitted) { }

public class ChedAh01ScenarioGenerator2()
    : HoldScenarioGenerator("cheda", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.Submitted) { }

public class ChedAh01ScenarioGenerator3()
    : HoldScenarioGenerator("cheda", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.InProgress) { }

public class ChedAh01ScenarioGenerator4()
    : HoldScenarioGenerator("cheda", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.InProgress) { }

public class ChedPh01ScenarioGenerator1()
    : HoldScenarioGenerator("chedp", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.Submitted) { }

public class ChedPh01ScenarioGenerator2()
    : HoldScenarioGenerator("chedp", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.Submitted) { }

public class ChedPh01ScenarioGenerator3()
    : HoldScenarioGenerator("chedp", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.InProgress) { }

public class ChedPh01ScenarioGenerator4()
    : HoldScenarioGenerator("chedp", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.InProgress) { }

public class ChedDh01ScenarioGenerator1()
    : HoldScenarioGenerator("chedd", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.Submitted) { }

public class ChedDh01ScenarioGenerator2()
    : HoldScenarioGenerator("chedd", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.Submitted) { }

public class ChedDh01ScenarioGenerator3()
    : HoldScenarioGenerator("chedd", InspectionRequiredEnum.NotRequired, ImportNotificationStatusEnum.InProgress) { }

public class ChedDh01ScenarioGenerator4()
    : HoldScenarioGenerator("chedd", InspectionRequiredEnum.Inconclusive, ImportNotificationStatusEnum.InProgress) { }

public class ChedAh02ScenarioGenerator1()
    : HoldScenarioGenerator("cheda", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.Submitted) { }

public class ChedAh02ScenarioGenerator2()
    : HoldScenarioGenerator("cheda", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.InProgress) { }

public class ChedDh02ScenarioGenerator1()
    : HoldScenarioGenerator("chedd", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.Submitted) { }

public class ChedDh02ScenarioGenerator2()
    : HoldScenarioGenerator("chedd", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.InProgress) { }

public class ChedPh02ScenarioGenerator1()
    : HoldScenarioGenerator("chedp", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.Submitted) { }

public class ChedPh02ScenarioGenerator2()
    : HoldScenarioGenerator("chedp", InspectionRequiredEnum.Required, ImportNotificationStatusEnum.InProgress) { }



public abstract class HoldScenarioGenerator(string filename, InspectionRequiredEnum inspectionRequired, ImportNotificationStatusEnum importNotificationStatus )
    : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var importNotification = GetNotificationBuilder($"AllChedsWithDecision/IPAFFS/{filename}")
            .WithInspectionStatus(inspectionRequired)
            .WithImportNotificationStatus(importNotificationStatus)
            .ValidateAndBuild();    
        
        var clearanceRequest = GetClearanceRequestBuilder($"AllChedsWithDecision/ALVS/cr-{filename}-match") 
            .ValidateAndBuild();
        
        return new GeneratorResult([
            clearanceRequest, 
            importNotification, 
        ]);
    }
}

public class ChedAc03ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("cheda", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableForInternalMarket }) { }

public class ChedPc03ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("chedp", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableForInternalMarket }) { }

public class ChedDc03ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("chedd", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableForInternalMarket }) { }

public class ChedAc05ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("cheda", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableForTemporaryImport }) { }

public class ChedAc06ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("cheda", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.HorseReEntry }) { }

public class ChedPc06ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("chedp", new Decision { ConsignmentAcceptable = true, DecisionEnum = DecisionDecisionEnum.AcceptableIfChanneled }) { }

public class ChedAn02ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("cheda", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Euthanasia }) { }

public class ChedAn04ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("cheda", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Reexport }) { }

public class ChedDn02ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("chedd", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Destruction }) { }

public class ChedDn04ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("chedd", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Redispatching }) { }

public class ChedPn02ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("chedp", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Destruction }) { }

public class ChedPn03ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("chedp", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Transformation }) { }

public class ChedPn04ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("chedp", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Reexport }) { }

public class ChedPn07ScenarioGenerator()
    : ReleaseAndRefusalScenarioGenerator("chedp", new Decision { ConsignmentAcceptable = false, NotAcceptableAction = DecisionNotAcceptableActionEnum.Other }) { }

public abstract class ReleaseAndRefusalScenarioGenerator(string filename, Decision decision)
    : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var importNotification = GetNotificationBuilder($"AllChedsWithDecision/IPAFFS/{filename}")
            .WithImportNotificationStatus(ImportNotificationStatusEnum.Validated)
            .WithPartTwoDecision(decision)
            .ValidateAndBuild();    
        
        var clearanceRequest = GetClearanceRequestBuilder($"AllChedsWithDecision/ALVS/cr-{filename}-match") 
            .ValidateAndBuild();
        
        return new GeneratorResult([
            clearanceRequest, 
            importNotification, 
        ]);
    }
}
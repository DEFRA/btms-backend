using CdmsBackend.Cli.Features.GenerateModels.DescriptorModel;
using CdmsBackend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;
using SharpYaml.Serialization;
using System.Text.Json;
using Newtonsoft.Json.Serialization;

namespace CdmsBackend.Cli.Features.GenerateModels.ClassMaps;

static class Bootstrap
{
    public static void GeneratorClassMaps()
    {
        RegisterAlvsClassMaps();
        RegisterIpaffsClassMaps();
        RegisterIpaffsEnumMaps();

        RegisterVehicleMovementsClassMaps();
    }

    public static void RegisterAlvsClassMaps()
    {
        GeneratorClassMap.RegisterClassMap("Header", map =>
        {
            map.MapProperty("ArrivalDateTime").IsDateTime().SetInternalName("ArrivedAt");
            map.MapProperty("MasterUCR").SetName("MasterUcr");
            map.MapProperty("SubmitterTURN").SetName("SubmitterTurn");
            map.MapProperty("DeclarationUCR").SetName("DeclarationUcr");
        });

        
        GeneratorClassMap.RegisterClassMap("ServiceHeader",
            map => { map.MapProperty("ServiceCallTimestamp").IsDateTime().SetInternalName("ServiceCalled"); });

        GeneratorClassMap.RegisterClassMap("ALVSClearanceRequest",
            map => { map.SetClassName("AlvsClearanceRequest"); });

        GeneratorClassMap.RegisterClassMap("ALVSClearanceRequestPost", map =>
        {
            map.SetClassName("AlvsClearanceRequestPost");
            map.MapProperty("AlvsClearanceRequest").SetType("AlvsClearanceRequest");
            map.MapProperty("sendingDate").SetInternalName("SentOn").IsDateTime();
        });

        GeneratorClassMap.RegisterClassMap("ALVSClearanceRequestPostResult", map =>
        {
            map.SetClassName("AlvsClearanceRequestPostResult")
                .NoInternalClass();
            map.MapProperty("sendingDate").SetInternalName("SentOn").IsDateTime();
        });
    }

    public static void RegisterIpaffsEnumMaps()
    {
        GeneratorEnumMap.RegisterEnumMap("ImportNotificationStatusEnum",
            map => { map.RemoveEnumValue("SUBMITTED,IN_PROGRESS,MODIFY"); });

        GeneratorEnumMap.RegisterEnumMap("purposeGroup",
            map => { map.AddEnumValue("For Import Non-Internal Market"); });
    }

    public static void RegisterIpaffsClassMaps()
    {
        GeneratorClassMap.RegisterClassMap("Decision",
            map =>
            {
                map.MapProperty("decision").SetName("DecisionEnum");
                map.MapProperty("notAcceptableActionByDate").IsDate();
            });

        GeneratorClassMap.RegisterClassMap("ImportNotification", map =>
        {
            map.MapProperty("Id").SetName("IpaffsId");
            map.MapProperty("Type").SetName("ImportNotificationType");
            map.MapProperty("LastUpdated").SetName("UpdatedSource").IsDateTime();
            map.MapProperty("RiskDecisionLockingTime").SetName("RiskDecisionLockedOn").IsDateTime();
        });

        GeneratorClassMap.RegisterClassMap("Purpose", map =>
        {
            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("estimatedArrivalDateAtPortOfExit",
                "estimatedArrivalTimeAtPortOfExit", "estimatedArrivedAtPortOfExit");


            map.MapProperty("exitDate").IsDate();
            map.MapProperty("FinalBIP").SetName("FinalBip");
            map.MapProperty("ExitBIP").SetName("ExitBip");
        });

        GeneratorClassMap.RegisterClassMap("AccompanyingDocument",
            map => { map.MapProperty("documentIssueDate").IsDateTime().SetInternalName("documentIssuedOn"); });

        GeneratorClassMap.RegisterClassMap("VeterinaryInformation",
            map =>
            {
                map.MapProperty("veterinaryDocumentIssueDate").IsDate().SetInternalName("veterinaryDocumentIssuedOn");
            });

        GeneratorClassMap.RegisterClassMap("InspectionOverride",
            map => { map.MapProperty("overriddenOn").IsDateTime(); });

        GeneratorClassMap.RegisterClassMap("SealCheck",
            map => { map.MapProperty("dateTimeOfCheck").IsDateTime().SetInternalName("checkedOn"); });

        GeneratorClassMap.RegisterClassMap("LaboratoryTests",
            map => { map.MapProperty("testDate").IsDateTime().SetInternalName("testedOn"); });

        GeneratorClassMap.RegisterClassMap("LaboratoryTestResult", map =>
        {
            map.MapProperty("releasedDate").IsDateTime().SetInternalName("releasedOn");
            map.MapProperty("labTestCreatedDate").IsDateTime().SetInternalName("labTestCreatedOn");
        });

        GeneratorClassMap.RegisterClassMap("DetailsOnReExport", map =>
        {
            map.MapProperty("date").IsDateTime();
            map.MapProperty("exitBIP").SetName("exitBip");
        });

        GeneratorClassMap.RegisterClassMap("CatchCertificateDetails",
            map => { map.MapProperty("dateOfIssue").IsDateTime().SetInternalName("issuedOn"); });

        GeneratorClassMap.RegisterClassMap("JourneyRiskCategorisationResult",
            map => { map.MapProperty("riskLevelDateTime").SetName("RiskLevelSetFor").IsDateTime(); });


        GeneratorClassMap.RegisterClassMap("RiskAssessmentResult",
            map => { map.MapProperty("assessmentDateTime").IsDateTime().SetInternalName("assessedOn"); });

        GeneratorClassMap.RegisterClassMap("Notification", map =>
        {
            map.MapProperty("isGMRMatched").SetName("isGmrMatched");
            map.MapProperty("riskDecisionLockingTime").IsDateTime();
            map.MapProperty("decisionDate").IsDateTime().SetInternalName("decisionOn");
            map.MapProperty("lastUpdated").IsDateTime();
            map.MapProperty("referenceNumber").SetBsonIgnore();
        });

        GeneratorClassMap.RegisterClassMap("CommodityComplement", map =>
        {
            map.AddProperty(new PropertyDescriptor("additionalData", "additionalData", "IDictionary<string, object>",
                "", false, false,
                IpaffsDescriptorBuilder.ClassNamePrefix));

            map.AddProperty(new PropertyDescriptor("riskAssesment", "riskAssesment", "CommodityRiskResult", "",
                true, false,
                IpaffsDescriptorBuilder.ClassNamePrefix));

            map.AddProperty(new PropertyDescriptor("checks", "checks", "InspectionCheck", "", true, true,
                IpaffsDescriptorBuilder.ClassNamePrefix));
        });

        GeneratorClassMap.RegisterClassMap("Commodities", map =>
        {
            map.MapProperty("complementParameterSet").SetBsonIgnore();
            map.MapProperty("commodityComplement").SetBsonIgnore();
        });


        GeneratorClassMap.RegisterClassMap("PartOne", map =>
        {
            map.MapProperty("commodities").ExcludeFromInternal();
            map.MapProperty("originalEstimatedDateTime").SetName("originalEstimatedOn").IsDateTime();
            map.MapProperty("submissionDate").SetName("SubmittedOn").IsDateTime();
            map.MapProperty("isGVMSRoute").SetName("isGvmsRoute");
            map.MapProperty("portOfExitDate").IsDateTime().SetInternalName("ExitedPortOfOn");

            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("arrivalDate", "arrivalTime", "arrivedOn");
            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("departureDate", "departureTime", "departedOn");
        });

        GeneratorClassMap.RegisterClassMap("Applicant", map =>
        {
            map.MapProperty("sampleDate").IsDate();
            map.MapProperty("sampleTime").IsTime();
            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("sampleDate", "sampleTime", "sampledOn");
        });

        GeneratorClassMap.RegisterClassMap("PartTwo", map =>
        {
            map.MapProperty("commodityChecks").ExcludeFromInternal();
            map.MapProperty("autoClearedDateTime").IsDateTime().SetInternalName("autoClearedOn");
            map.MapProperty("checkDate").IsDateTime().SetInternalName("checkedOn");
        });

        GeneratorClassMap.RegisterClassMap("PartThree", map => { map.MapProperty("destructionDate").IsDate(); });


        GeneratorClassMap.RegisterClassMap("ComplementParameterSet", map =>
        {
            map.MapProperty("KeyDataPair")
                .SetType("IDictionary<string, object>")
                .AddAttribute("[JsonConverter(typeof(KeyDataPairsToDictionaryStringObjectJsonConverter))]",
                    Model.Source)
                .SetMapper("Cdms.Types.Ipaffs.Mapping.DictionaryMapper");
        });


        GeneratorClassMap.RegisterClassMap("EconomicOperator", map =>
        {
            map.MapProperty("individualName").IsSensitive();
            map.MapProperty("companyName").IsSensitive();
        });

        GeneratorClassMap.RegisterClassMap("Address", map =>
        {
            map.MapProperty("Street").IsSensitive();
            map.MapProperty("City").IsSensitive();
            map.MapProperty("postalCode").IsSensitive();
            map.MapProperty("addressLine1").IsSensitive();
            map.MapProperty("addressLine2").IsSensitive();
            map.MapProperty("addressLine3").IsSensitive();
            map.MapProperty("postalZipCode").IsSensitive();
            map.MapProperty("email").IsSensitive();
            map.MapProperty("ukTelephone").IsSensitive();
            map.MapProperty("telephone").IsSensitive();
            map.MapProperty("countryISOCode").SetName("countryIsoCode");
        });


        GeneratorClassMap.RegisterClassMap("Party", map =>
        {
            map.MapProperty("email").IsSensitive();
            map.MapProperty("fax").IsSensitive();
            map.MapProperty("phone").IsSensitive();
            map.MapProperty("city").IsSensitive();
            map.MapProperty("postCode").IsSensitive();
            map.MapProperty("Address").IsSensitive();
            map.MapProperty("companyName").IsSensitive();
            map.MapProperty("name").IsSensitive();
        });

        GeneratorClassMap.RegisterClassMap("UserInformation", map => { map.MapProperty("displayName").IsSensitive(); });

        GeneratorClassMap.RegisterClassMap("OfficialVeterinarian", map =>
        {
            map.MapProperty("firstName").IsSensitive();
            map.MapProperty("lastName").IsSensitive();
            map.MapProperty("email").IsSensitive();
            map.MapProperty("phone").IsSensitive();
            map.MapProperty("fax").IsSensitive();
        });
    }


    public static void RegisterVehicleMovementsClassMaps()
    {
        GeneratorClassMap.RegisterClassMap("GmrsByVRN",
            map => { map.SetClassName("GmrsByVrn"); });

        GeneratorClassMap.RegisterClassMap("gmrs", map =>
        {
            map.SetClassName("Gmr");
            map.MapProperty("gmrId").SetInternalName("id");
            map.MapProperty("haulierEORI").SetName("haulierEori");
            map.MapProperty("vehicleRegNum").SetName("vehicleRegistrationNumber");
            map.MapProperty("updatedDateTime").SetSourceName("LastUpdated").SetInternalName("UpdatedSource").IsDateTime();
            map.MapProperty("localDateTimeOfDeparture").SetName("departsAt").IsDateTime();
            map.MapProperty("declarations").ExcludeFromInternal();
        });

        GeneratorClassMap.RegisterClassMap("SearchGmrsForDeclarationIdsResponse",
            map => { map.MapProperty("Gmrs").SetType("Gmr[]"); });

        GeneratorClassMap.RegisterClassMap("SearchGmrsForVRNsresponse",
            map =>
            {
                map.MapProperty("Gmrs").SetType("Gmr[]");
                map.MapProperty("gmrsByVRN").SetName("gmrsByVrns").SetType("GmrsByVrn[]");
            });

        GeneratorClassMap.RegisterClassMap("searchGmrsResponse", map => { map.MapProperty("Gmrs").SetType("Gmr[]"); });


        GeneratorClassMap.RegisterClassMap("plannedCrossing",
            map =>
            {
                map.MapProperty("localDateTimeOfArrival").IsDateTime().SetName("arrivesAt");
                map.MapProperty("localDateTimeOfDeparture").IsDateTime().SetName("departsAt");
            });

        GeneratorClassMap.RegisterClassMap("actualCrossing",
            map => { map.MapProperty("localDateTimeOfArrival").IsDateTime().SetName("arrivesAt"); });

        GeneratorClassMap.RegisterClassMap("checkedInCrossing",
            map => { map.MapProperty("localDateTimeOfArrival").IsDateTime().SetName("arrivesAt"); });
    }
}
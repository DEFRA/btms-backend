using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;

namespace Btms.Backend.Cli.Features.GenerateModels.ClassMaps;

internal static class Bootstrap
{
    //Not ideal that the casing doesn't match, but the coding styles mandate:
    private const string ArrivesAt = "arrivesAt";
    private const string AlvsClearanceRequest = "AlvsClearanceRequest";
    private const string CdsClearanceRequest = "CdsClearanceRequest";


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
            map.MapProperty("ArrivalDateTime")
                .IsDateTime(DatetimeType.Epoch)
                .SetInternalName(ArrivesAt);

            map.MapProperty("MasterUCR")
                .SetName("masterUcr")
                .SetSourceJsonPropertyName("masterUCR");

            map.MapProperty("SubmitterTURN")
                .SetName("submitterTurn")
                .SetSourceJsonPropertyName("submitterTURN");

            map.MapProperty("DeclarationUCR")
                .SetName("declarationUcr")
                .SetSourceJsonPropertyName("declarationUCR");

        });

        GeneratorClassMap.RegisterClassMap("Items", map =>
        {
            map.MapProperty("Document")
                .SetSourceJsonPropertyName("documents")
                .SetInternalJsonPropertyName("documents");

            map.MapProperty("Check")
                .SetSourceJsonPropertyName("checks")
                .SetInternalJsonPropertyName("checks");
        });

        GeneratorClassMap.RegisterClassMap("Check", map =>
        {
            map.AddProperty(new PropertyDescriptor("decisionCode", "string", false, false));

            map.AddProperty(new PropertyDescriptor("decisionsValidUntil", "DateTime", false, false));

            map.AddProperty(new PropertyDescriptor("decisionReasons", "string", false, true));

            map.MapProperty("MasterUCR")
                .SetName("masterUcr")
                .SetSourceJsonPropertyName("masterUCR");

            map.MapProperty("SubmitterTURN")
                .SetName("submitterTurn")
                .SetSourceJsonPropertyName("submitterTURN");

            map.MapProperty("DeclarationUCR")
                .SetName("declarationUcr")
                .SetSourceJsonPropertyName("declarationUCR");
        });

        GeneratorClassMap.RegisterClassMap("ServiceHeader",
            map =>
            {
                map.MapProperty("ServiceCallTimestamp").IsDateTime(DatetimeType.Epoch)
                .SetInternalName("serviceCalled");
            });

        GeneratorClassMap.RegisterClassMap(AlvsClearanceRequest,
            map => { map.SetClassName(AlvsClearanceRequest, CdsClearanceRequest); });

        GeneratorClassMap.RegisterClassMap($"{AlvsClearanceRequest}Post", map =>
        {
            map.SetClassName($"{AlvsClearanceRequest}Post", $"{CdsClearanceRequest}Post");
            map.MapProperty(AlvsClearanceRequest).SetType(AlvsClearanceRequest, CdsClearanceRequest);
            map.MapProperty("sendingDate").SetInternalName("sentOn").IsDateTime();
        });

        GeneratorClassMap.RegisterClassMap($"{AlvsClearanceRequest}PostResult", map =>
        {
            map.SetClassName($"{AlvsClearanceRequest}PostResult", $"{CdsClearanceRequest}PostResult")
                .ExcludeFromInternal();
            map.MapProperty("sendingDate").SetInternalName("sentOn").IsDateTime();
        });
    }

    public static void RegisterIpaffsEnumMaps()
    {
        GeneratorEnumMap.RegisterEnumMap("ImportNotificationStatusEnum",
            map => { map.RemoveEnumValue("SUBMITTED,IN_PROGRESS,MODIFY"); });

        GeneratorEnumMap.RegisterEnumMap("PurposePurposeGroupEnum",
            map => { map.AddEnumValue("For Import Non-Internal Market"); });
    }

    public static void RegisterIpaffsClassMaps()
    {
        GeneratorClassMap.RegisterClassMap("AccompanyingDocument",
            map =>
            {
                map.MapProperty("documentIssueDate").IsDateTime().SetInternalName("documentIssuedOn");
            });

        GeneratorClassMap.RegisterClassMap("Applicant",
            map =>
            {
                map.MapProperty("sampleDate").IsDate();
                map.MapProperty("sampleTime").IsTime();
                map.MapDateOnlyAndTimeOnlyToDateTimeProperty("sampleDate", "sampleTime", "sampledOn");
            });

        GeneratorClassMap.RegisterClassMap("Decision",
            map =>
            {
                map.MapProperty("decision").SetName("decisionEnum");
                map.MapProperty("notAcceptableActionByDate").IsDate();
            });

        GeneratorClassMap.RegisterClassMap("ImportNotification", map =>
        {
            map.MapProperty("Id").SetName("ipaffsId");
            map.MapProperty("Type").SetName("importNotificationType");
            map.MapProperty("LastUpdated").SetName("lastUpdated", "UpdatedSource").IsDateTime();
            map.MapProperty("RiskDecisionLockingTime").SetName("riskDecisionLockedOn").IsDateTime();
            map.MapProperty("RiskAssessment").ExcludeFromInternal();
        });

        GeneratorClassMap.RegisterClassMap("Purpose", map =>
        {
            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("estimatedArrivalDateAtPortOfExit",
                "estimatedArrivalTimeAtPortOfExit", "estimatedArrivesAtPortOfExit");


            map.MapProperty("exitDate").IsDate();
            map.MapProperty("FinalBIP").SetName("finalBip");
            map.MapProperty("ExitBIP").SetName("exitBip");
        });

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

        GeneratorClassMap.RegisterClassMap("CatchCertificateAttachment",
            map =>
            {
                map.MapProperty("CatchCertificateDetails")
                    .SetName("catchCertificateDetails")
                    .SetSourceJsonPropertyName("CatchCertificateDetails");
            });

        GeneratorClassMap.RegisterClassMap("CatchCertificateDetails",
            map => { map.MapProperty("dateOfIssue").IsDateTime().SetInternalName("issuedOn"); });

        GeneratorClassMap.RegisterClassMap("JourneyRiskCategorisationResult",
            map => { map.MapProperty("riskLevelDateTime").SetName("riskLevelSetFor").IsDateTime(); });


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
            map.AddProperty(new PropertyDescriptor("additionalData", "IDictionary<string, object>", false, false));

            map.AddProperty(new PropertyDescriptor("riskAssesment", "CommodityRiskResult", true, false));

            map.AddProperty(new PropertyDescriptor("checks", "InspectionCheck", true, true));
        });

        GeneratorClassMap.RegisterClassMap("Commodities", map =>
        {
            map.MapProperty("complementParameterSet").ExcludeFromInternal();
            map.MapProperty("commodityComplement").ExcludeFromInternal();
        });


        GeneratorClassMap.RegisterClassMap("PartOne", map =>
        {
            map.MapProperty("commodities").ExcludeFromInternal();
            map.MapProperty("originalEstimatedDateTime").SetName("originalEstimatedOn").IsDateTime();
            map.MapProperty("submissionDate").SetName("submittedOn").IsDateTime();
            map.MapProperty("isGVMSRoute").SetName("isGvmsRoute");
            map.MapProperty("portOfExitDate").IsDateTime().SetInternalName("exitedPortOfOn");

            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("arrivalDate", "arrivalTime", ArrivesAt);
            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("departureDate", "departureTime", "departedOn");
        });

        GeneratorClassMap.RegisterClassMap("PartTwo", map =>
        {
            map.MapProperty("commodityChecks").ExcludeFromInternal();
            map.MapProperty("autoClearedDateTime").IsDateTime().SetInternalName("autoClearedOn");
            map.MapProperty("checkDate").IsDateTime().SetInternalName("checkedOn");
            map.MapProperty("inspectionRequired").SetType("InspectionRequiredEnum")
                .SetMapper("InspectionRequiredEnumMapper");
        });

        GeneratorClassMap.RegisterClassMap("PartThree", map => { map.MapProperty("destructionDate").IsDate(); });


        GeneratorClassMap.RegisterClassMap("ComplementParameterSet", map =>
        {
            map.ExcludeFromInternal();

            map.MapProperty("KeyDataPair")
                .SetType("IDictionary<string, object>")
                .AddAttribute("[JsonConverter(typeof(KeyDataPairsToDictionaryStringObjectJsonConverter))]",
                    Model.Source)
                .SetMapper("Btms.Types.Ipaffs.Mapping.DictionaryMapper");
        });
    }


    public static void RegisterVehicleMovementsClassMaps()
    {
        GeneratorClassMap.RegisterClassMap("GmrsByVRN",
            map =>
            {
                map.SetClassName("GmrsByVrn");
                map.ExcludeFromInternal();
            });

        GeneratorClassMap.RegisterClassMap("gmrs", map =>
        {
            map.SetClassName("Gmr");
            map.MapProperty("gmrId").SetInternalName("id");
            map.MapProperty("haulierEORI").SetName("haulierEori");

            map.MapProperty("vehicleRegNum").SetName("vehicleRegistrationNumber");
            map.MapProperty("updatedDateTime").SetName("updatedSource").IsDateTime();
            map.MapProperty("localDateTimeOfDeparture").SetName("departsAt").IsDateTime();
            map.MapProperty("declarations").ExcludeFromInternal();
        });

        GeneratorClassMap.RegisterClassMap("SearchGmrsForDeclarationIdsRequest",
            map => { map.ExcludeFromInternal(); });

        GeneratorClassMap.RegisterClassMap("SearchGmrsForDeclarationIdsResponse",
            map => { map.ExcludeFromInternal(); map.MapProperty("Gmrs").SetType("Gmr[]"); });

        GeneratorClassMap.RegisterClassMap("SearchGmrsForVRNrequest",
            map => { map.ExcludeFromInternal(); });

        GeneratorClassMap.RegisterClassMap("SearchGmrsRequest",
            map => { map.ExcludeFromInternal(); });

        GeneratorClassMap.RegisterClassMap("SearchGmrsForVRNsresponse",
            map =>
            {
                map.SetClassName("SearchGmrsForVrnsResponse");
                map.ExcludeFromInternal();
                map.MapProperty("Gmrs").SetType("Gmr[]");
                map.MapProperty("gmrsByVRN").SetName("gmrsByVrns").SetType("GmrsByVrn[]");
            });

        GeneratorClassMap.RegisterClassMap("searchGmrsResponse", map =>
        {
            map.ExcludeFromInternal();
            map.MapProperty("Gmrs").SetType("Gmr[]");
        });


        GeneratorClassMap.RegisterClassMap("plannedCrossing",
            map =>
            {
                map.MapProperty("localDateTimeOfArrival").IsDateTime().SetName(ArrivesAt);
                map.MapProperty("localDateTimeOfDeparture").IsDateTime().SetName("departsAt");
            });

        GeneratorClassMap.RegisterClassMap("actualCrossing",
            map => { map.MapProperty("localDateTimeOfArrival").IsDateTime().SetName(ArrivesAt); });

        GeneratorClassMap.RegisterClassMap("checkedInCrossing",
            map => { map.MapProperty("localDateTimeOfArrival").IsDateTime().SetName(ArrivesAt); });
    }
}
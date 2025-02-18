using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;

namespace Btms.Backend.Cli.Features.GenerateModels.ClassMaps;

internal static class Bootstrap
{
    //Not ideal that the casing doesn't match, but the coding styles mandate:
    private const string ArrivesAt = "arrivesAt";
    private const string AlvsClearanceRequest = "AlvsClearanceRequest";
    private const string CdsClearanceRequest = "CdsClearanceRequest";

    // We know these fields are going to be in GB so we should know the timezone, based on BST/DST
    // but its not certain IPAFFS behaves reliably on this, so for now we're assuming unknown timezone
    private const DateTimeType IpaffsKnownGb = DateTimeType.Local;

    // These fields are Z in the timestamp 
    // but its not certain IPAFFS behaves reliably on this, so for now we're assuming unknown timezone
    private const DateTimeType IpaffsUtc = DateTimeType.Utc;

    // These fields have no TZ indication in the timestamp so we don't know which timezone it is.
    // We could in future use location info to determine it.
    private const DateTimeType IpaffsNoTzInfo = DateTimeType.Local;

    // We don't have any examples of these fields
    private const DateTimeType IpaffsNoExamples = DateTimeType.Local;

    // We don't have any examples of these fields, but the dates we've seen in ALVS have all been Epoch
    private const DateTimeType AlvsNoExamples = DateTimeType.Epoch;

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
                .IsDateTime(DateTimeType.Epoch)
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
                map.MapProperty("ServiceCallTimestamp").IsDateTime(DateTimeType.Epoch)
                .SetInternalName("serviceCalled");
            });

        GeneratorClassMap.RegisterClassMap(AlvsClearanceRequest,
            map => { map.SetClassName(AlvsClearanceRequest, CdsClearanceRequest); });

        GeneratorClassMap.RegisterClassMap($"{AlvsClearanceRequest}Post", map =>
        {
            map.SetClassName($"{AlvsClearanceRequest}Post", $"{CdsClearanceRequest}Post");
            map.MapProperty(AlvsClearanceRequest).SetType(AlvsClearanceRequest, CdsClearanceRequest);
            map.MapProperty("sendingDate")
                .SetInternalName("sentOn").IsDateTime(AlvsNoExamples);
        });

        GeneratorClassMap.RegisterClassMap($"{AlvsClearanceRequest}PostResult", map =>
        {
            map.SetClassName($"{AlvsClearanceRequest}PostResult", $"{CdsClearanceRequest}PostResult")
                .ExcludeFromInternal();
            map.MapProperty("sendingDate").SetInternalName("sentOn").IsDateTime(DateTimeType.Epoch);
        });
    }

    private static void RegisterIpaffsEnumMaps()
    {
        GeneratorEnumMap.RegisterEnumMap("ImportNotificationStatusEnum",
            map => { map.RemoveEnumValue("SUBMITTED,IN_PROGRESS,MODIFY"); });

        GeneratorEnumMap.RegisterEnumMap("PurposePurposeGroupEnum",
            map => { map.AddEnumValue("For Import Non-Internal Market"); });
    }

    private static void RegisterIpaffsClassMaps()
    {
        GeneratorClassMap.RegisterClassMap("SealContainer",
            map =>
            {
                map.MapProperty("sealNumber").IsSensitive();
                map.MapProperty("containerNumber").IsSensitive();
                map.MapProperty("resealedSealNumber").IsSensitive();
            });

        GeneratorClassMap.RegisterClassMap("IdentificationDetails",
            map =>
            {
                map.MapProperty("identificationDetail").IsSensitive();
                map.MapProperty("identificationDescription").IsSensitive();
            });

        GeneratorClassMap.RegisterClassMap("AccompanyingDocument",
            map =>
            {
                map.MapProperty("documentReference").IsSensitive();
                map.MapProperty("attachmentId").IsSensitive();
                map.MapProperty("attachmentFilename").IsSensitive();
                map.MapProperty("uploadUserId").IsSensitive();
                map.MapProperty("uploadOrganisationId").IsSensitive();

                map.MapProperty("documentIssueDate")
                    .SetInternalName("documentIssuedOn")
                    .IsDate(DateOnlyType.Flexible);
            });

        GeneratorClassMap.RegisterClassMap("MeansOfTransport",
            map =>
            {
                map.MapProperty("document").IsSensitive();
                map.MapProperty("id").IsSensitive();
            });

        GeneratorClassMap.RegisterClassMap("Identifiers",
            map =>
            {
                map.MapProperty("data").IsSensitive();
            });

        GeneratorClassMap.RegisterClassMap("ContactDetails",
            map =>
            {
                map.MapProperty("name").IsSensitive();
                map.MapProperty("telephone").IsSensitive();
                map.MapProperty("email").IsSensitive();
                map.MapProperty("agent").IsSensitive();
            });

        GeneratorClassMap.RegisterClassMap("Applicant",
            map =>
            {
                map.MapProperty("laboratoryAddress").IsSensitive();
                map.MapProperty("laboratoryIdentification").IsSensitive();
                map.MapProperty("laboratoryPhoneNumber").IsSensitive();
                map.MapProperty("laboratoryEmail").IsSensitive();
                map.MapProperty("sampleBatchNumber").IsSensitive();
                map.MapProperty("laboratory").IsSensitive();
                map.MapProperty("sampleType").IsSensitive();
                map.MapProperty("sampleDate").IsDate();
                map.MapProperty("sampleTime").IsTime();
                map.MapDateOnlyAndTimeOnlyToDateTimeProperty("sampleDate", "sampleTime",
                    "sampledOn", IpaffsNoTzInfo);
            });

        GeneratorClassMap.RegisterClassMap("BillingInformation",
            map =>
            {
                map.MapProperty("emailAddress").IsSensitive();
                map.MapProperty("phoneNumber").IsSensitive();
                map.MapProperty("contactName").IsSensitive();
            });

        GeneratorClassMap.RegisterClassMap("ApprovedEstablishment",
            map =>
            {
                map.MapProperty("name").IsSensitive();
                map.MapProperty("approvalNumber").IsSensitive();
            });

        GeneratorClassMap.RegisterClassMap("PostalAddress",
            map =>
            {
                map.MapProperty("addressLine1").IsSensitive();
                map.MapProperty("addressLine2").IsSensitive();
                map.MapProperty("addressLine3").IsSensitive();
                map.MapProperty("addressLine4").IsSensitive();
                map.MapProperty("county").IsSensitive();
                map.MapProperty("cityOrTown").IsSensitive();
                map.MapProperty("postalCode").IsSensitive();
            });

        GeneratorClassMap.RegisterClassMap("Inspector",
            map =>
            {
                map.MapProperty("name").IsSensitive();
                map.MapProperty("phone").IsSensitive();
                map.MapProperty("email").IsSensitive();
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
            map.MapProperty("LastUpdated").SetName("lastUpdated", "UpdatedSource")
                .IsDateTime(IpaffsUtc);

            map.MapProperty("RiskDecisionLockingTime").SetName("riskDecisionLockedOn")
                .IsDateTime(IpaffsUtc);

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
            map => { map.MapProperty("overriddenOn").IsDateTime(IpaffsNoExamples); });

        GeneratorClassMap.RegisterClassMap("SealCheck",
            map => { map.MapProperty("dateTimeOfCheck").IsDateTime(IpaffsNoExamples).SetInternalName("checkedOn"); });

        GeneratorClassMap.RegisterClassMap("LaboratoryTests",
            map => { map.MapProperty("testDate").IsDateTime(IpaffsNoTzInfo).SetInternalName("testedOn"); });

        GeneratorClassMap.RegisterClassMap("LaboratoryTestResult", map =>
        {
            map.MapProperty("releasedDate").IsDate().SetInternalName("releasedOn");
            map.MapProperty("labTestCreatedDate").IsDate().SetInternalName("labTestCreatedOn");
            map.MapProperty("results").IsSensitive();
            map.MapProperty("laboratoryTestMethod").IsSensitive();
        });

        GeneratorClassMap.RegisterClassMap("DetailsOnReExport", map =>
        {
            map.MapProperty("date").IsDate();
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
            map => { map.MapProperty("dateOfIssue").SetInternalName("issuedOn").IsDate(); });

        GeneratorClassMap.RegisterClassMap("JourneyRiskCategorisationResult",
            map =>
            {
                map.MapProperty("riskLevelDateTime").SetName("riskLevelSetFor")
                .IsDateTime(IpaffsNoExamples);
            });


        GeneratorClassMap.RegisterClassMap("RiskAssessmentResult",
            map =>
            {
                map.MapProperty("assessmentDateTime").SetInternalName("assessedOn")
                .IsDateTime(IpaffsNoExamples);
            });

        GeneratorClassMap.RegisterClassMap("Notification", map =>
        {
            map.MapProperty("isGMRMatched").SetName("isGmrMatched");
            map.MapProperty("riskDecisionLockingTime").IsDateTime(IpaffsUtc);
            map.MapProperty("decisionDate").SetInternalName("decisionOn").IsDateTime(IpaffsUtc);
            map.MapProperty("lastUpdated").IsDateTime(IpaffsUtc);
            map.MapProperty("referenceNumber").SetBsonIgnore();
        });

        GeneratorClassMap.RegisterClassMap("CommodityComplement", map =>
        {
            map.MapProperty("speciesName").IsSensitive();
            map.MapProperty("speciesID").IsSensitive();
            map.MapProperty("speciesNomination").IsSensitive();
            map.MapProperty("speciesType").IsSensitive();
            map.MapProperty("speciesClassName").IsSensitive();
            map.MapProperty("speciesClass").IsSensitive();
            map.MapProperty("speciesFamilyName").IsSensitive();
            map.MapProperty("speciesFamily").IsSensitive();
            map.MapProperty("speciesCommonName").IsSensitive();

            map.AddProperty(new PropertyDescriptor("additionalData", "IDictionary<string, object>",
                false, false));

            map.AddProperty(new PropertyDescriptor("riskAssesment", "CommodityRiskResult",
                true, false));

            map.AddProperty(new PropertyDescriptor("checks", "InspectionCheck",
                true, true));
        });

        GeneratorClassMap.RegisterClassMap("Commodities", map =>
        {
            map.MapProperty("complementParameterSet").ExcludeFromInternal();
            map.MapProperty("commodityComplement").ExcludeFromInternal();
        });


        GeneratorClassMap.RegisterClassMap("PartOne", map =>
        {
            map.MapProperty("importerLocalReferenceNumber").IsSensitive();
            map.MapProperty("commodities").ExcludeFromInternal();
            map.MapProperty("originalEstimatedDateTime").SetName("originalEstimatedOn")
                .IsDateTime(IpaffsUtc);

            map.MapProperty("submissionDate").SetName("submittedOn")
                .IsDateTime(IpaffsUtc);

            map.MapProperty("isGVMSRoute").SetName("isGvmsRoute");
            map.MapProperty("portOfExitDate").SetInternalName("exitedPortOfOn")
                .IsDateTime(IpaffsNoExamples);

            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("arrivalDate", "arrivalTime",
                ArrivesAt, IpaffsKnownGb);

            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("departureDate", "departureTime",
                "departedOn", IpaffsNoTzInfo);
        });

        GeneratorClassMap.RegisterClassMap("PartTwo", map =>
        {
            map.MapProperty("bipLocalReferenceNumber").IsSensitive();
            map.MapProperty("commodityChecks").ExcludeFromInternal();
            map.MapProperty("autoClearedDateTime").SetInternalName("autoClearedOn")
                .IsDateTime(IpaffsUtc);
            map.MapProperty("checkDate").SetInternalName("checkedOn")
                .IsDateTime(IpaffsUtc);
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


        GeneratorClassMap.RegisterClassMap("EconomicOperator", map =>
        {
            map.MapProperty("individualName").IsSensitive();
            map.MapProperty("companyName").IsSensitive();
            map.MapProperty("approvalNumber").IsSensitive();
            map.MapProperty("otherIdentifier").IsSensitive();
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
            map.MapProperty("countryISOCode").SetName("countryIsoCode").IsSensitive();
        });

        GeneratorClassMap.RegisterClassMap("OfficialInspector", map =>
        {
            map.MapProperty("firstName").IsSensitive();
            map.MapProperty("lastName").IsSensitive();
            map.MapProperty("email").IsSensitive();
            map.MapProperty("phone").IsSensitive();
            map.MapProperty("fax").IsSensitive();
        });

        GeneratorClassMap.RegisterClassMap("NominatedContact", map =>
        {
            map.MapProperty("name").IsSensitive();
            map.MapProperty("email").IsSensitive();
            map.MapProperty("telephone").IsSensitive();
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

    private static void RegisterVehicleMovementsClassMaps()
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
            map.MapProperty("updatedDateTime").SetName("updatedSource")
                .IsDateTime(DateTimeType.Utc);

            map.MapProperty("localDateTimeOfDeparture").SetName("departsAt")
                .IsDateTime(DateTimeType.Local);

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
                map.MapProperty("localDateTimeOfDeparture")
                    .SetName("departsAt").IsDateTime(DateTimeType.Local);
            });

        GeneratorClassMap.RegisterClassMap("actualCrossing",
            map =>
            {
                map.MapProperty("localDateTimeOfArrival")
                    .SetName(ArrivesAt).IsDateTime(DateTimeType.Local);
            });

        GeneratorClassMap.RegisterClassMap("checkedInCrossing",
            map =>
            {
                map.MapProperty("localDateTimeOfArrival")
                    .SetName(ArrivesAt).IsDateTime(DateTimeType.Local);
            });
    }
}
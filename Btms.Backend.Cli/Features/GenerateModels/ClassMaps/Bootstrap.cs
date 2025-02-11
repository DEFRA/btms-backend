using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;

namespace Btms.Backend.Cli.Features.GenerateModels.ClassMaps;

internal static class Bootstrap
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
            map.MapProperty("ArrivalDateTime")
                .IsDateTime(DatetimeType.Epoch)
                .SetInternalName("arrivesAt");

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
            map.AddProperty(new PropertyDescriptor("decisionCode", "decisionCode", "string",
                "", false, false,
                IpaffsDescriptorBuilder.ClassNamePrefix));

            map.AddProperty(new PropertyDescriptor("decisionsValidUntil", "decisionsValidUntil", "DateTime",
                "", false, false,
                IpaffsDescriptorBuilder.ClassNamePrefix));

            map.AddProperty(new PropertyDescriptor("decisionReasons", "decisionReasons", "string",
                "", false, true,
                IpaffsDescriptorBuilder.ClassNamePrefix));


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
            map => { map.MapProperty("ServiceCallTimestamp").IsDateTime(DatetimeType.Epoch).SetInternalName("serviceCalled"); });

        GeneratorClassMap.RegisterClassMap("AlvsClearanceRequest",
            map => { map.SetClassName("AlvsClearanceRequest", "CdsClearanceRequest"); });

        GeneratorClassMap.RegisterClassMap("AlvsClearanceRequestPost", map =>
        {
            map.SetClassName("AlvsClearanceRequestPost", "CdsClearanceRequestPost");
            map.MapProperty("AlvsClearanceRequest").SetType("AlvsClearanceRequest", "CdsClearanceRequest");
            map.MapProperty("sendingDate").SetInternalName("sentOn").IsDateTime();
        });

        GeneratorClassMap.RegisterClassMap("AlvsClearanceRequestPostResult", map =>
        {
            map.SetClassName("AlvsClearanceRequestPostResult", "CdsClearanceRequestPostResult")
                .NoInternalClass();
            map.MapProperty("sendingDate").SetInternalName("sentOn").IsDateTime();
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
                map.MapProperty("documentIssueDate").IsDateTime().SetInternalName("documentIssuedOn");
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
                map.MapDateOnlyAndTimeOnlyToDateTimeProperty("sampleDate", "sampleTime", "sampledOn");
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
            map.MapProperty("LastUpdated").SetName("lastUpdated", "UpdatedSource").IsDateTime();
            map.MapProperty("RiskDecisionLockingTime").SetName("riskDecisionLockedOn").IsDateTime();
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
            map.MapProperty("results").IsSensitive();
            map.MapProperty("laboratoryTestMethod").IsSensitive();
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
            map.MapProperty("speciesName").IsSensitive();
            map.MapProperty("speciesID").IsSensitive();
            map.MapProperty("speciesNomination").IsSensitive();
            map.MapProperty("speciesType").IsSensitive();
            map.MapProperty("speciesClassName").IsSensitive();
            map.MapProperty("speciesClass").IsSensitive();
            map.MapProperty("speciesFamilyName").IsSensitive();
            map.MapProperty("speciesFamily").IsSensitive();
            map.MapProperty("speciesCommonName").IsSensitive();


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
            map.MapProperty("importerLocalReferenceNumber").IsSensitive();
            map.MapProperty("commodities").ExcludeFromInternal();
            map.MapProperty("originalEstimatedDateTime").SetName("originalEstimatedOn").IsDateTime();
            map.MapProperty("submissionDate").SetName("submittedOn").IsDateTime();
            map.MapProperty("isGVMSRoute").SetName("isGvmsRoute");
            map.MapProperty("portOfExitDate").IsDateTime().SetInternalName("exitedPortOfOn");

            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("arrivalDate", "arrivalTime", "arrivesAt");
            map.MapDateOnlyAndTimeOnlyToDateTimeProperty("departureDate", "departureTime", "departedOn");
        });

        GeneratorClassMap.RegisterClassMap("PartTwo", map =>
        {
            map.MapProperty("bipLocalReferenceNumber").IsSensitive();
            map.MapProperty("commodityChecks").ExcludeFromInternal();
            map.MapProperty("autoClearedDateTime").IsDateTime().SetInternalName("autoClearedOn");
            map.MapProperty("checkDate").IsDateTime().SetInternalName("checkedOn");
            map.MapProperty("inspectionRequired").SetType("InspectionRequiredEnum")
                .SetMapper("InspectionRequiredEnumMapper");
        });

        GeneratorClassMap.RegisterClassMap("PartThree", map => { map.MapProperty("destructionDate").IsDate(); });


        GeneratorClassMap.RegisterClassMap("ComplementParameterSet", map =>
        {
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
            map.MapProperty("updatedDateTime").SetName("updatedSource").IsDateTime();
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
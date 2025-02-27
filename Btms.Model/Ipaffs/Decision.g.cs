//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

using JsonApiDotNetCore.Resources.Annotations;
using System.Text.Json.Serialization;
using System.Dynamic;


namespace Btms.Model.Ipaffs;

/// <summary>
/// Decision if the consignment can be imported
/// </summary>
public partial class Decision  //
{


    /// <summary>
    /// Is consignment acceptable or not
    /// </summary>
    [Attr]
    [JsonPropertyName("consignmentAcceptable")]
    [System.ComponentModel.Description("Is consignment acceptable or not")]
    public bool? ConsignmentAcceptable { get; set; }


    /// <summary>
    /// Filled if consignmentAcceptable is set to false
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableAction")]
    [System.ComponentModel.Description("Filled if consignmentAcceptable is set to false")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionNotAcceptableActionEnum? NotAcceptableAction { get; set; }


    /// <summary>
    /// Filled if not acceptable action is set to destruction
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableActionDestructionReason")]
    [System.ComponentModel.Description("Filled if not acceptable action is set to destruction")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionNotAcceptableActionDestructionReasonEnum? NotAcceptableActionDestructionReason { get; set; }


    /// <summary>
    /// Filled if not acceptable action is set to entry refusal
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableActionEntryRefusalReason")]
    [System.ComponentModel.Description("Filled if not acceptable action is set to entry refusal")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionNotAcceptableActionEntryRefusalReasonEnum? NotAcceptableActionEntryRefusalReason { get; set; }


    /// <summary>
    /// Filled if not acceptable action is set to quarantine imposed
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableActionQuarantineImposedReason")]
    [System.ComponentModel.Description("Filled if not acceptable action is set to quarantine imposed")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionNotAcceptableActionQuarantineImposedReasonEnum? NotAcceptableActionQuarantineImposedReason { get; set; }


    /// <summary>
    /// Filled if not acceptable action is set to special treatment
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableActionSpecialTreatmentReason")]
    [System.ComponentModel.Description("Filled if not acceptable action is set to special treatment")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionNotAcceptableActionSpecialTreatmentReasonEnum? NotAcceptableActionSpecialTreatmentReason { get; set; }


    /// <summary>
    /// Filled if not acceptable action is set to industrial processing
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableActionIndustrialProcessingReason")]
    [System.ComponentModel.Description("Filled if not acceptable action is set to industrial processing")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionNotAcceptableActionIndustrialProcessingReasonEnum? NotAcceptableActionIndustrialProcessingReason { get; set; }


    /// <summary>
    /// Filled if not acceptable action is set to re-dispatch
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableActionReDispatchReason")]
    [System.ComponentModel.Description("Filled if not acceptable action is set to re-dispatch")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionNotAcceptableActionReDispatchReasonEnum? NotAcceptableActionReDispatchReason { get; set; }


    /// <summary>
    /// Filled if not acceptable action is set to use for other purposes
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableActionUseForOtherPurposesReason")]
    [System.ComponentModel.Description("Filled if not acceptable action is set to use for other purposes")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionNotAcceptableActionUseForOtherPurposesReasonEnum? NotAcceptableActionUseForOtherPurposesReason { get; set; }


    /// <summary>
    /// Filled when notAcceptableAction is equal to destruction
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableDestructionReason")]
    [System.ComponentModel.Description("Filled when notAcceptableAction is equal to destruction")]
    public string? NotAcceptableDestructionReason { get; set; }


    /// <summary>
    /// Filled when notAcceptableAction is equal to other
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableActionOtherReason")]
    [System.ComponentModel.Description("Filled when notAcceptableAction is equal to other")]
    public string? NotAcceptableActionOtherReason { get; set; }


    /// <summary>
    /// Filled when consignmentAcceptable is set to false
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableActionByDate")]
    [System.ComponentModel.Description("Filled when consignmentAcceptable is set to false")]
    public DateOnly? NotAcceptableActionByDate { get; set; }


    /// <summary>
    /// List of details for individual chedpp not acceptable reasons
    /// </summary>
    [Attr]
    [JsonPropertyName("chedppNotAcceptableReasons")]
    [System.ComponentModel.Description("List of details for individual chedpp not acceptable reasons")]
    public ChedppNotAcceptableReason[]? ChedppNotAcceptableReasons { get; set; }


    /// <summary>
    /// If the consignment was not accepted what was the reason
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableReasons")]
    [System.ComponentModel.Description("If the consignment was not accepted what was the reason")]
    public string[]? NotAcceptableReasons { get; set; }


    /// <summary>
    /// 2 digits ISO code of country (not acceptable country can be empty)
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableCountry")]
    [System.ComponentModel.Description("2 digits ISO code of country (not acceptable country can be empty)")]
    public string? NotAcceptableCountry { get; set; }


    /// <summary>
    /// Filled if consignmentAcceptable is set to false
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableEstablishment")]
    [System.ComponentModel.Description("Filled if consignmentAcceptable is set to false")]
    public string? NotAcceptableEstablishment { get; set; }


    /// <summary>
    /// Filled if consignmentAcceptable is set to false
    /// </summary>
    [Attr]
    [JsonPropertyName("notAcceptableOtherReason")]
    [System.ComponentModel.Description("Filled if consignmentAcceptable is set to false")]
    public string? NotAcceptableOtherReason { get; set; }


    /// <summary>
    /// Details of controlled destinations
    /// </summary>
    [Attr]
    [JsonPropertyName("detailsOfControlledDestinations")]
    [System.ComponentModel.Description("Details of controlled destinations")]
    public Party? DetailsOfControlledDestinations { get; set; }


    /// <summary>
    /// Filled if consignment is set to acceptable and decision type is Specific Warehouse
    /// </summary>
    [Attr]
    [JsonPropertyName("specificWarehouseNonConformingConsignment")]
    [System.ComponentModel.Description("Filled if consignment is set to acceptable and decision type is Specific Warehouse")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionSpecificWarehouseNonConformingConsignmentEnum? SpecificWarehouseNonConformingConsignment { get; set; }


    /// <summary>
    /// Deadline when consignment has to leave borders
    /// </summary>
    [Attr]
    [JsonPropertyName("temporaryDeadline")]
    [System.ComponentModel.Description("Deadline when consignment has to leave borders")]
    public string? TemporaryDeadline { get; set; }


    /// <summary>
    /// Detailed decision for consignment
    /// </summary>
    [Attr]
    [JsonPropertyName("decisionEnum")]
    [System.ComponentModel.Description("Detailed decision for consignment")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionDecisionEnum? DecisionEnum { get; set; }


    /// <summary>
    /// Decision over purpose of free circulation in country
    /// </summary>
    [Attr]
    [JsonPropertyName("freeCirculationPurpose")]
    [System.ComponentModel.Description("Decision over purpose of free circulation in country")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionFreeCirculationPurposeEnum? FreeCirculationPurpose { get; set; }


    /// <summary>
    /// Decision over purpose of definitive import
    /// </summary>
    [Attr]
    [JsonPropertyName("definitiveImportPurpose")]
    [System.ComponentModel.Description("Decision over purpose of definitive import")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionDefinitiveImportPurposeEnum? DefinitiveImportPurpose { get; set; }


    /// <summary>
    /// Decision channeled option based on (article8, article15)
    /// </summary>
    [Attr]
    [JsonPropertyName("ifChanneledOption")]
    [System.ComponentModel.Description("Decision channeled option based on (article8, article15)")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionIfChanneledOptionEnum? IfChanneledOption { get; set; }


    /// <summary>
    /// Custom warehouse registered number
    /// </summary>
    [Attr]
    [JsonPropertyName("customWarehouseRegisteredNumber")]
    [System.ComponentModel.Description("Custom warehouse registered number")]
    public string? CustomWarehouseRegisteredNumber { get; set; }


    /// <summary>
    /// Free warehouse registered number
    /// </summary>
    [Attr]
    [JsonPropertyName("freeWarehouseRegisteredNumber")]
    [System.ComponentModel.Description("Free warehouse registered number")]
    public string? FreeWarehouseRegisteredNumber { get; set; }


    /// <summary>
    /// Ship name
    /// </summary>
    [Attr]
    [JsonPropertyName("shipName")]
    [System.ComponentModel.Description("Ship name")]
    public string? ShipName { get; set; }


    /// <summary>
    /// Port of exit
    /// </summary>
    [Attr]
    [JsonPropertyName("shipPortOfExit")]
    [System.ComponentModel.Description("Port of exit")]
    public string? ShipPortOfExit { get; set; }


    /// <summary>
    /// Ship supplier registered number
    /// </summary>
    [Attr]
    [JsonPropertyName("shipSupplierRegisteredNumber")]
    [System.ComponentModel.Description("Ship supplier registered number")]
    public string? ShipSupplierRegisteredNumber { get; set; }


    /// <summary>
    /// Transhipment BIP
    /// </summary>
    [Attr]
    [JsonPropertyName("transhipmentBip")]
    [System.ComponentModel.Description("Transhipment BIP")]
    public string? TranshipmentBip { get; set; }


    /// <summary>
    /// Transhipment third country
    /// </summary>
    [Attr]
    [JsonPropertyName("transhipmentThirdCountry")]
    [System.ComponentModel.Description("Transhipment third country")]
    public string? TranshipmentThirdCountry { get; set; }


    /// <summary>
    /// Transit exit BIP
    /// </summary>
    [Attr]
    [JsonPropertyName("transitExitBip")]
    [System.ComponentModel.Description("Transit exit BIP")]
    public string? TransitExitBip { get; set; }


    /// <summary>
    /// Transit third country
    /// </summary>
    [Attr]
    [JsonPropertyName("transitThirdCountry")]
    [System.ComponentModel.Description("Transit third country")]
    public string? TransitThirdCountry { get; set; }


    /// <summary>
    /// Transit destination third country
    /// </summary>
    [Attr]
    [JsonPropertyName("transitDestinationThirdCountry")]
    [System.ComponentModel.Description("Transit destination third country")]
    public string? TransitDestinationThirdCountry { get; set; }


    /// <summary>
    /// Temporary exit BIP
    /// </summary>
    [Attr]
    [JsonPropertyName("temporaryExitBip")]
    [System.ComponentModel.Description("Temporary exit BIP")]
    public string? TemporaryExitBip { get; set; }


    /// <summary>
    /// Horse re-entry
    /// </summary>
    [Attr]
    [JsonPropertyName("horseReentry")]
    [System.ComponentModel.Description("Horse re-entry")]
    public string? HorseReentry { get; set; }


    /// <summary>
    /// Is it transshipped to EU or third country (values EU / country name)
    /// </summary>
    [Attr]
    [JsonPropertyName("transhipmentEuOrThirdCountry")]
    [System.ComponentModel.Description("Is it transshipped to EU or third country (values EU / country name)")]
    public string? TranshipmentEuOrThirdCountry { get; set; }

}
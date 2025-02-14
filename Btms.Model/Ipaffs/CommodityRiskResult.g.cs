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
/// Result of the risk assessment of a commodity
/// </summary>
public partial class CommodityRiskResult  //
{


    /// <summary>
    /// CHED-A, CHED-D, CHED-P - what is the commodity complement risk decision
    /// </summary>
    [Attr]
    [JsonPropertyName("riskDecision")]
    [System.ComponentModel.Description("CHED-A, CHED-D, CHED-P - what is the commodity complement risk decision")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public CommodityRiskResultRiskDecisionEnum? RiskDecision { get; set; }


    /// <summary>
    /// Transit CHED - what is the commodity complement exit risk decision
    /// </summary>
    [Attr]
    [JsonPropertyName("exitRiskDecision")]
    [System.ComponentModel.Description("Transit CHED - what is the commodity complement exit risk decision")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public CommodityRiskResultExitRiskDecisionEnum? ExitRiskDecision { get; set; }


    /// <summary>
    /// HMI decision required
    /// </summary>
    [Attr]
    [JsonPropertyName("hmiDecision")]
    [System.ComponentModel.Description("HMI decision required")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public CommodityRiskResultHmiDecisionEnum? HmiDecision { get; set; }


    /// <summary>
    /// PHSI decision required
    /// </summary>
    [Attr]
    [JsonPropertyName("phsiDecision")]
    [System.ComponentModel.Description("PHSI decision required")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public CommodityRiskResultPhsiDecisionEnum? PhsiDecision { get; set; }


    /// <summary>
    /// PHSI classification
    /// </summary>
    [Attr]
    [JsonPropertyName("phsiClassification")]
    [System.ComponentModel.Description("PHSI classification")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public CommodityRiskResultPhsiClassificationEnum? PhsiClassification { get; set; }


    /// <summary>
    /// PHSI Decision Breakdown
    /// </summary>
    [Attr]
    [JsonPropertyName("phsi")]
    [System.ComponentModel.Description("PHSI Decision Breakdown")]
    public Phsi? Phsi { get; set; }


    /// <summary>
    /// UUID used to match to the complement parameter set
    /// </summary>
    [Attr]
    [JsonPropertyName("uniqueId")]
    [System.ComponentModel.Description("UUID used to match to the complement parameter set")]
    public string? UniqueId { get; set; }


    /// <summary>
    /// EPPO Code for the species
    /// </summary>
    [Attr]
    [JsonPropertyName("eppoCode")]
    [System.ComponentModel.Description("EPPO Code for the species")]
    public string? EppoCode { get; set; }


    /// <summary>
    /// Name or ID of the variety
    /// </summary>
    [Attr]
    [JsonPropertyName("variety")]
    [System.ComponentModel.Description("Name or ID of the variety")]
    public string? Variety { get; set; }


    /// <summary>
    /// Whether or not a plant is woody
    /// </summary>
    [Attr]
    [JsonPropertyName("isWoody")]
    [System.ComponentModel.Description("Whether or not a plant is woody")]
    public bool? IsWoody { get; set; }


    /// <summary>
    /// Indoor or Outdoor for a plant
    /// </summary>
    [Attr]
    [JsonPropertyName("indoorOutdoor")]
    [System.ComponentModel.Description("Indoor or Outdoor for a plant")]
    public string? IndoorOutdoor { get; set; }


    /// <summary>
    /// Whether the propagation is considered a Plant, Bulb, Seed or None
    /// </summary>
    [Attr]
    [JsonPropertyName("propagation")]
    [System.ComponentModel.Description("Whether the propagation is considered a Plant, Bulb, Seed or None")]
    public string? Propagation { get; set; }


    /// <summary>
    /// Rule type for PHSI checks
    /// </summary>
    [Attr]
    [JsonPropertyName("phsiRuleType")]
    [System.ComponentModel.Description("Rule type for PHSI checks")]
    public string? PhsiRuleType { get; set; }

}
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
using System.Runtime.Serialization;
using Btms.Model.Auditing;
using Btms.Model.Ipaffs;
using MongoDB.Bson.Serialization.Attributes;


namespace Btms.Model.Cds;

public partial class ItemCheck
{
    [Attr]
    [System.ComponentModel.Description("")]
    public int ItemNumber { get; set; }
    
    [Attr]
    [System.ComponentModel.Description("")]
    public required string CheckCode { get; set; }
    
    [Attr]
    [System.ComponentModel.Description("")]
    public required string AlvsDecisionCode { get; set; }

    [Attr]
    [System.ComponentModel.Description("")]
    public string? BtmsDecisionCode { get; set; }
}

public class DecisionImportNotifications
{
    public required string Id { get; set; }
    public required int? Version { get; set; }
    public required DateTime Created { get; set; }
    public required DateTime Updated { get; set; }
    public required DateTime UpdatedEntity { get; set; }
    public required DateTime CreatedSource { get; set; }
    public required DateTime UpdatedSource { get; set; }
}

public class StatusChecker
{

    [Attr]
    [System.ComponentModel.Description("")]
    public bool AllMatch { get; set; } = default;
        
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AnyMatch { get; set; } = default;
        
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AllNoMatch { get; set; } = default;
        
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AnyNoMatch { get; set; } = default;
        
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AllHold { get; set; } = default;
        
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AnyHold { get; set; } = default;
        
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AllRefuse { get; set; } = default;
        
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AnyRefuse { get; set; } = default;
        
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AllRelease { get; set; } = default;
        
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AnyRelease { get; set; } = default;
}

[JsonConverter(typeof(JsonStringEnumConverterEx<MovementStatusEnum>))]
public enum MovementStatusEnum 
{
    [EnumMember(Value = "Decision Match")]
    DecisionMatch = 0,
    
    [EnumMember(Value = "Feature Missing")]
    FeatureMissing = 1,

    [EnumMember(Value = "Investigation Needed")]
    InvestigationNeeded = -1,
    
    [EnumMember(Value = "Known Issue")]
    KnownIssue = -2,
    
    [EnumMember(Value = "Data Issue")]
    DataIssue = -10,

}


[JsonConverter(typeof(JsonStringEnumConverterEx<MovementSegmentEnum>))]
public enum MovementSegmentEnum 
{
    // CHED-PP PHSI
    [EnumMember(Value = "CDMS-205 AC1")]
    Cdms205Ac1,
    [EnumMember(Value = "CDMS-205 AC2")]
    Cdms205Ac2,
    [EnumMember(Value = "CDMS-205 AC3")]
    Cdms205Ac3,
    [EnumMember(Value = "CDMS-205 AC4")]
    Cdms205Ac4,
    [EnumMember(Value = "CDMS-205 AC5")]
    Cdms205Ac5,
    
    //Errors
    [EnumMember(Value = "CDMS-249")]
    Cdms249,
    
    None,
}

public class MovementStatus
{   
    public static MovementStatus Default()
    {
        return new MovementStatus()
        {
            ChedTypes = [],
            LinkStatus = LinkStatusEnum.NotLinked
        };
    }
    
    [Attr]
    [System.ComponentModel.Description("")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public required ImportNotificationTypeEnum[] ChedTypes { get; set; }

    // [Attr]
    // [System.ComponentModel.Description("")]
    // public required bool Linked { get; set; }
    
    [Attr]
    [System.ComponentModel.Description("")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public required LinkStatusEnum LinkStatus { get; set; }
    
    [Attr]
    [System.ComponentModel.Description("")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string? LinkStatusDescription { get; set; }

    [Attr]
    [System.ComponentModel.Description("")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public MovementStatusEnum? Status { get; set; }

    [Attr]
    [System.ComponentModel.Description("")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public MovementSegmentEnum? Segment { get; set; } 
}

[JsonConverter(typeof(JsonStringEnumConverterEx<LinkStatusEnum>))]
public enum LinkStatusEnum
{
    [EnumMember(Value = "Error")]
    Error,
    
    [EnumMember(Value = "Not Linked")]
    NotLinked,
    
    [EnumMember(Value = "Partially Linked")]
    PartiallyLinked,
    
    [EnumMember(Value = "Missing Links")]
    MissingLinks,
    
    [EnumMember(Value = "No Links")]
    NoLinks,
    
    [EnumMember(Value = "All Linked")]
    AllLinked,
    
    [EnumMember(Value = "Investigate")]
    Investigate
}

[JsonConverter(typeof(JsonStringEnumConverterEx<DecisionStatusEnum>))]
public enum DecisionStatusEnum 
{
    [EnumMember(Value = "Btms Made Same Decision As Alvs")]
    BtmsMadeSameDecisionAsAlvs,
    
    [EnumMember(Value = "CDMS-205")]
    ReliesOnCDMS205,
    
    [EnumMember(Value = "CDMS-249")]
    ReliesOnCDMS249,
    
    [EnumMember(Value = "Has Ched PP Checks")]
    HasChedppChecks,
    
    [EnumMember(Value = "No Import Notifications Linked")]
    NoImportNotificationsLinked,
    
    [EnumMember(Value = "Has Other E9X Data Errors")]
    HasOtherDataErrors,
    
    [EnumMember(Value = "Has Generic E99 Data Errors")]
    HasGenericDataErrors,
    
    [EnumMember(Value = "No Alvs Decisions")]
    NoAlvsDecisions,
    
    [EnumMember(Value = "Has Multiple Ched Types")]
    HasMultipleChedTypes,
    
    [EnumMember(Value = "Has Multiple Cheds")]
    HasMultipleCheds,
    
    [EnumMember(Value = "Investigation Needed")]
    InvestigationNeeded,
    
    [EnumMember(Value = "None")]
    None,
}

public partial class SummarisedDecisionContext : AuditContext //
{
    [Attr]
    [System.ComponentModel.Description("")]
    public int? AlvsDecisionNumber { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public int EntryVersionNumber { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public List<DecisionImportNotifications>? ImportNotifications { get; set; }
    
    [Attr]
    [System.ComponentModel.Description("")]
    public DecisionComparison? DecisionComparison { get; set; }
}

public class DecisionComparison
{
    [Attr]
    [System.ComponentModel.Description("")]
    public bool Paired { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DecisionStatusEnum DecisionStatus { get; set; }
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool DecisionMatched { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public int? BtmsDecisionNumber { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public List<ItemCheck> Checks { get; set; } = new List<ItemCheck>();
}

public partial class DecisionContext : SummarisedDecisionContext //
{
    [Attr]
    [System.ComponentModel.Description("")]
    public StatusChecker? AlvsCheckStatus { get; set; }
    
    [Attr]
    [System.ComponentModel.Description("")]
    public StatusChecker? BtmsCheckStatus { get; set; }
}

public partial class AlvsDecisionStatus  //
{
    [Attr]
    [System.ComponentModel.Description("")]
    public List<AlvsDecision> Decisions { get; set; } = new List<AlvsDecision>();

    // [Attr]
    // [System.ComponentModel.Description("")]
    // [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    // public DecisionStatusEnum DecisionStatus { get; set; } = DecisionStatusEnum.NoAlvsDecisions;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public SummarisedDecisionContext Context { get; set; } = new SummarisedDecisionContext()
    {
        //Initialise the DecisionComparison at the top level
        DecisionComparison = new DecisionComparison()
        {
            DecisionStatus = DecisionStatusEnum.NoAlvsDecisions
        }
    };
}

public partial class AlvsDecision  //
{
    [Attr]
    [System.ComponentModel.Description("")]
    public required CdsClearanceRequest Decision { get; set; }

    [Attr]
    [System.ComponentModel.Description("")]
    public required DecisionContext Context { get; set; }
}



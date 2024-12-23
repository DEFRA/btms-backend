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
using Btms.Model.Auditing;


namespace Btms.Model.Cds;

/// <summary>
/// 
/// </summary>
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


/// <summary>
/// 
/// </summary>
public partial class DecisionContext : IAuditContext //
{
    [Attr]
    [System.ComponentModel.Description("")]
    public int AlvsDecisionNumber { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public int BtmsDecisionNumber { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public int EntryVersionNumber { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool Paired { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public string? DecisionStatus { get; set; }
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool DecisionMatched { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool BtmsAllNoMatch { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool BtmsAnyNoMatch { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool BtmsAllHold { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool BtmsAnyHold { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool BtmsAllRefuse { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool BtmsAnyRefuse { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool BtmsAllRelease { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool BtmsAnyRelease { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AlvsAllNoMatch { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AlvsAnyNoMatch { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AlvsAllHold { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AlvsAnyHold { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AlvsAllRefuse { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AlvsAnyRefuse { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AlvsAllRelease { get; set; } = default;
    
    [Attr]
    [System.ComponentModel.Description("")]
    public bool AlvsAnyRelease { get; set; } = default;
    
}

/// <summary>
/// 
/// </summary>
public partial class AlvsDecision  //
{
    [Attr]
    [System.ComponentModel.Description("")]
    public required CdsClearanceRequest Decision { get; set; }

    [Attr]
    [System.ComponentModel.Description("")]
    public required DecisionContext Context { get; set; }
    
    // TODO - should we put this into context, and so into audit log?
    [Attr]
    [System.ComponentModel.Description("")]
    public required List<ItemCheck> Checks { get; set; }
    
}



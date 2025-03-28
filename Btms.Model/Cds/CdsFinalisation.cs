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

public partial class CdsFinalisation : AuditContext
{

    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public ServiceHeader? ServiceHeader { get; set; }

    [Attr]
    [System.ComponentModel.Description("")]
    public required FinalisationHeader Header { get; set; }
}

public enum FinalState
{
    // From https://eaflood.atlassian.net/wiki/spaces/ALVS/pages/5176590480/FinalState+Field
    Cleared = 0,
    CancelledAfterArrival = 1,
    CancelledWhilePreLodged = 2,
    Destroyed = 3,
    Seized = 4,
    ReleasedToKingsWarehouse = 5,
    TransferredToMss = 6
}

public static class FinalStateExtensions
{
    public static bool IsCancelled(this FinalState finalState)
    {
        return finalState == FinalState.CancelledAfterArrival || finalState == FinalState.CancelledWhilePreLodged;
    }

    public static bool IsNotCancelled(this FinalState finalState)
    {
        return !finalState.IsCancelled();
    }
}

public partial class FinalisationHeader  //
{

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("entryReference")]
    public required string EntryReference { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("entryVersionNumber")]
    public required int EntryVersionNumber { get; set; }

    /// <summary>
    /// 
    /// </summary
    [JsonPropertyName("decisionNumber")]
    public int? DecisionNumber { get; set; }

    /// <summary>
    /// 
    /// </summary
    [JsonPropertyName("finalState")]
    public required FinalState FinalState { get; set; }

    /// <summary>
    /// 
    /// </summary
    [JsonPropertyName("manualAction")]
    public required bool ManualAction { get; set; }

}
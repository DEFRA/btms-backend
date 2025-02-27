//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

using System.Text.Json.Serialization;
using System.Dynamic;


namespace Btms.Types.Ipaffs;


/// <summary>
/// Information about not acceptable reason
/// </summary>
public partial class ChedppNotAcceptableReason  //
{


    /// <summary>
    /// reason for refusal
    /// </summary>
    [JsonPropertyName("reason")]
    public ChedppNotAcceptableReasonReasonEnum? Reason { get; set; }


    /// <summary>
    /// commodity or package
    /// </summary>
    [JsonPropertyName("commodityOrPackage")]
    public ChedppNotAcceptableReasonCommodityOrPackageEnum? CommodityOrPackage { get; set; }

}
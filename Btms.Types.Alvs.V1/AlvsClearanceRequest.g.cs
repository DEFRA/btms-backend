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


namespace Btms.Types.Alvs;

/// <summary>
/// 
/// </summary>
public partial class AlvsClearanceRequest  //
{


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("serviceHeader")]
    public ServiceHeader? ServiceHeader { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("header")]
    public Header? Header { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("items")]
    public Items[]? Items { get; set; }

}
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

public partial class AlvsClearanceRequest  //
{

    [JsonPropertyName("serviceHeader")]
    public ServiceHeader? ServiceHeader { get; set; }

    [JsonPropertyName("header")]
    public Header? Header { get; set; }

    [JsonPropertyName("items")]
    public Items[]? Items { get; set; }

}
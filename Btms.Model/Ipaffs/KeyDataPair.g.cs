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
public partial class KeyDataPair  //
{

    [Attr]
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [Attr]
    [JsonPropertyName("data")]
    public string? Data { get; set; }

}
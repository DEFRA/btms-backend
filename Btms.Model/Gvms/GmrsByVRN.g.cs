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


namespace Btms.Model.Gvms;
public partial class GmrsByVrn  //
{


    /// <summary>
    /// This is the identifier for a Vehicle Registration Number
    /// </summary>
    [Attr]
    [JsonPropertyName("vrn")]
    [System.ComponentModel.Description("This is the identifier for a Vehicle Registration Number")]
    public string? Vrn { get; set; }

    [Attr]
    [JsonPropertyName("gmrs")]
    public string[]? Gmrs { get; set; }

}
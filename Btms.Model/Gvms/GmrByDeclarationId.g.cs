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
public partial class GmrByDeclarationId  //
{


    /// <summary>
    /// This is the identifier for a customs declaration from Customs Declaration Service (CDS) or CHIEF. For inbound movements declared in CDS it is a MRN, for example 19GB4S24GC3PPFGVR7. For inbound movements declared in CHIEF it is an ERN, for example 999123456C20210615. For outbound movements declared in either CDS or CHIEF it is a DUCR, for example 0GB689223596000-SE119404.
    /// </summary>
    [Attr]
    [JsonPropertyName("dec")]
    [System.ComponentModel.Description("This is the identifier for a customs declaration from Customs Declaration Service (CDS) or CHIEF. For inbound movements declared in CDS it is a MRN, for example 19GB4S24GC3PPFGVR7. For inbound movements declared in CHIEF it is an ERN, for example 999123456C20210615. For outbound movements declared in either CDS or CHIEF it is a DUCR, for example 0GB689223596000-SE119404.")]
    public string? Dec { get; set; }

    [Attr]
    [JsonPropertyName("gmrs")]
    public string[]? Gmrs { get; set; }

}
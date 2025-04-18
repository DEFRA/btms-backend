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
/// Message sent to the server to send an ALVSClearanceRequest.
/// </summary>
public partial class AlvsClearanceRequestPost  //
{

    [JsonPropertyName("xmlSchemaVersion")]
    public string? XmlSchemaVersion { get; set; }

    [JsonPropertyName("userIdentification")]
    public string? UserIdentification { get; set; }

    [JsonPropertyName("userPassword")]
    public string? UserPassword { get; set; }

    [JsonPropertyName("sendingDate")]
    [Btms.Common.Json.EpochDateTimeJsonConverter]
    public DateTime? SendingDate { get; set; }

    [JsonPropertyName("alvsClearanceRequest")]
    public AlvsClearanceRequest? AlvsClearanceRequest { get; set; }

}
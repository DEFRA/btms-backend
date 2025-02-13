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
/// Details of the seal check
/// </summary>
public partial class SealCheck  //
{


    /// <summary>
    /// Is seal check satisfactory
    /// </summary>
    [JsonPropertyName("satisfactory")]
    public bool? Satisfactory { get; set; }


    /// <summary>
    /// reason for not satisfactory
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }


    /// <summary>
    /// Official inspector
    /// </summary>
    [JsonPropertyName("officialInspector")]
    public OfficialInspector? OfficialInspector { get; set; }


    /// <summary>
    /// date and time of seal check
    /// </summary>
    [JsonPropertyName("dateTimeOfCheck")]
    public DateTime? DateTimeOfCheck { get; set; }

}
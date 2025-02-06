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


namespace Btms.Types.Gvms;

/// <summary>
/// 
/// </summary>
public partial class ActualCrossing  //
{


    /// <summary>
    /// The ID of the crossing route, using a routeId from the GVMS reference data
    /// </summary>
    [JsonPropertyName("routeId")]
    public string? RouteId { get; set; }


    /// <summary>
    /// The planned date and time of arrival, in local time of the arrival port. Must not include seconds, time zone or UTC marker
    /// </summary>
    [JsonPropertyName("localDateTimeOfArrival")]
    public string? LocalArrivesAt { get; set; }

}
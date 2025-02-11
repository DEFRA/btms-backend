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
/// Information about an inspection that is required
/// </summary>
public partial class ReportToLocations  //
{


    /// <summary>
    /// An inspectionTypeId from GVMS Reference Data denoting the type of inspection that needs to be performed on the vehicle.
    /// </summary>
    [JsonPropertyName("inspectionTypeId")]
    public string? InspectionTypeId { get; set; }


    /// <summary>
    /// A list of locationIds from GVMS Reference Data that are available to perform this type of inspection.
    /// </summary>
    [JsonPropertyName("locationIds")]
    public string[]? LocationIds { get; set; }

}
/*------------------------------------------------------------------------------
<auto-generated>
    This code was generated from the InternalClass template.
    Manual changes to this file may cause unexpected behavior in your application.
    Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
using JsonApiDotNetCore.Resources.Annotations;
using System.Text.Json.Serialization;
using System.Dynamic;

namespace Btms.Model.Gvms;

/// <summary>
/// Information about an inspection that is required
/// </summary>
public partial class ReportToLocations //
{

    /// <summary>
    /// An inspectionTypeId from GVMS Reference Data denoting the type of inspection that needs to be performed on the vehicle.
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("An inspectionTypeId from GVMS Reference Data denoting the type of inspection that needs to be performed on the vehicle.")]
    public string? InspectionTypeId { get; set; }
}

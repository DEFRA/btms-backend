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

namespace Btms.Model.Ipaffs;

/// <summary>
/// 
/// </summary>
public partial class CommodityChecks //
{

    /// <summary>
    /// UUID used to match the commodityChecks to the commodityComplement
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("UUID used to match the commodityChecks to the commodityComplement")]
    public string? UniqueComplementId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public InspectionCheck[]? Checks { get; set; }

    /// <summary>
    /// Manually entered validity period, allowed if risk decision is INSPECTION_REQUIRED and HMI check status &#x27;Compliant&#x27; or &#x27;Not inspected&#x27;
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Manually entered validity period, allowed if risk decision is INSPECTION_REQUIRED and HMI check status 'Compliant' or 'Not inspected'")]
    public int? ValidityPeriod { get; set; }
}

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
public partial class CatchCertificates //
{

    /// <summary>
    /// The catch certificate number
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("The catch certificate number")]
    public string? CertificateNumber { get; set; }

    /// <summary>
    /// The catch certificate weight number
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("The catch certificate weight number")]
    public double? Weight { get; set; }
}

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
/// Validation field code-message representation
/// </summary>
public partial class ValidationMessageCode //
{

    /// <summary>
    /// Field
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Field")]
    public string? Field { get; set; }

    /// <summary>
    /// Code
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Code")]
    public string? Code { get; set; }
}

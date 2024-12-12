/*------------------------------------------------------------------------------
<auto-generated>
    This code was generated from the Class template.
    Manual changes to this file may cause unexpected behavior in your application.
    Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
using System.Text.Json.Serialization;
using System.Dynamic;

namespace Btms.Types.Ipaffs;

/// <summary>
/// Control part of notification
/// </summary>
public partial class PartThree //
{

    /// <summary>
    /// Control status enum
    /// </summary>
    [JsonPropertyName("controlStatus")]
    public PartThreeControlStatusEnum? ControlStatus { get; set; }

    /// <summary>
    /// Control details
    /// </summary>
    [JsonPropertyName("control")]
    public Control? Control { get; set; }

    /// <summary>
    /// Validation messages for Part 3 - Control
    /// </summary>
    [JsonPropertyName("consignmentValidation")]
    public ValidationMessageCode[]? ConsignmentValidations { get; set; }

    /// <summary>
    /// Is the seal check required
    /// </summary>
    [JsonPropertyName("sealCheckRequired")]
    public bool? SealCheckRequired { get; set; }

    /// <summary>
    /// Seal check details
    /// </summary>
    [JsonPropertyName("sealCheck")]
    public SealCheck? SealCheck { get; set; }

    /// <summary>
    /// Seal check override details
    /// </summary>
    [JsonPropertyName("sealCheckOverride")]
    public InspectionOverride? SealCheckOverride { get; set; }
}

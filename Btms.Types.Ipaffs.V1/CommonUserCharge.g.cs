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
/// 
/// </summary>
public partial class CommonUserCharge //
{

    /// <summary>
    /// Indicates whether the last applicable change was successfully send over the interface to Trade Charge
    /// </summary>
    [JsonPropertyName("wasSentToTradeCharge")]
    public bool? WasSentToTradeCharge { get; set; }
}

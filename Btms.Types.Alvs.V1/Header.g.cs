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

namespace Btms.Types.Alvs;

/// <summary>
/// 
/// </summary>
public partial class Header //
{

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("entryReference")]
    public string? EntryReference { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("entryVersionNumber")]
    public int? EntryVersionNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("previousVersionNumber")]
    public int? PreviousVersionNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("declarationUCR")]
    public string? DeclarationUcr { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("declarationPartNumber")]
    public string? DeclarationPartNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("declarationType")]
    public string? DeclarationType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("arrivalDateTime")]
    public DateTime? ArrivalDateTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("submitterTURN")]
    public string? SubmitterTurn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("declarantId")]
    public string? DeclarantId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("declarantName")]
    public string? DeclarantName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("dispatchCountryCode")]
    public string? DispatchCountryCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("goodsLocationCode")]
    public string? GoodsLocationCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("masterUCR")]
    public string? MasterUcr { get; set; }
}

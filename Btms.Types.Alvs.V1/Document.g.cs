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


namespace Btms.Types.Alvs;

public partial class Document  //
{

    [JsonPropertyName("documentCode")]
    public string? DocumentCode { get; set; }

    [JsonPropertyName("documentReference")]
    public string? DocumentReference { get; set; }

    [JsonPropertyName("documentStatus")]
    public string? DocumentStatus { get; set; }

    [JsonPropertyName("documentControl")]
    public string? DocumentControl { get; set; }

    [JsonPropertyName("documentQuantity")]
    public decimal? DocumentQuantity { get; set; }

}
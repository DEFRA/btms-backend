//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

using JsonApiDotNetCore.Resources.Annotations;
using System.Text.Json.Serialization;
using System.Dynamic;


namespace Btms.Model.Cds;
public partial class Document  //
{

    [Attr]
    [JsonPropertyName("documentCode")]
    public string? DocumentCode { get; set; }

    [Attr]
    [JsonPropertyName("documentReference")]
    public string? DocumentReference { get; set; }

    [Attr]
    [JsonPropertyName("documentStatus")]
    public string? DocumentStatus { get; set; }

    [Attr]
    [JsonPropertyName("documentControl")]
    public string? DocumentControl { get; set; }

    [Attr]
    [JsonPropertyName("documentQuantity")]
    public decimal? DocumentQuantity { get; set; }

}
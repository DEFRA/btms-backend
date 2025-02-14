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


namespace Btms.Model.Ipaffs;

/// <summary>
/// Laboratory tests details
/// </summary>
public partial class LaboratoryTests  //
{


    /// <summary>
    /// Date of tests
    /// </summary>
    [Attr]
    [JsonPropertyName("testedOn")]
    [System.ComponentModel.Description("Date of tests")]
    public DateTime? TestedOn { get; set; }


    /// <summary>
    /// Reason for test
    /// </summary>
    [Attr]
    [JsonPropertyName("testReason")]
    [System.ComponentModel.Description("Reason for test")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public LaboratoryTestsTestReasonEnum? TestReason { get; set; }


    /// <summary>
    /// List of details of individual tests performed or to be performed
    /// </summary>
    [Attr]
    [JsonPropertyName("singleLaboratoryTests")]
    [System.ComponentModel.Description("List of details of individual tests performed or to be performed")]
    public SingleLaboratoryTest[]? SingleLaboratoryTests { get; set; }

}
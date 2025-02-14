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


namespace Btms.Types.Ipaffs;


/// <summary>
/// Laboratory tests details
/// </summary>
public partial class LaboratoryTests  //
{


    /// <summary>
    /// Date of tests
    /// </summary>
    [JsonPropertyName("testDate")]
    public DateTime? TestDate { get; set; }


    /// <summary>
    /// Reason for test
    /// </summary>
    [JsonPropertyName("testReason")]
    public LaboratoryTestsTestReasonEnum? TestReason { get; set; }


    /// <summary>
    /// List of details of individual tests performed or to be performed
    /// </summary>
    [JsonPropertyName("singleLaboratoryTests")]
    public SingleLaboratoryTest[]? SingleLaboratoryTests { get; set; }

}
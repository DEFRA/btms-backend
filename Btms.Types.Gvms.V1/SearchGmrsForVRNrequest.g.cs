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


namespace Btms.Types.Gvms;

public partial class SearchGmrsForVRNrequest  //
{


    ///<summary>
    /// A list of VRN to search for GMRs.
    /// </summary>
    [JsonPropertyName("vrns")]
    public string[]? Vrns { get; set; }

}
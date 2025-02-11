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
/// Person to be contacted if there is an issue with the consignment
/// </summary>
public partial class ContactDetails  //
{


    /// <summary>
    /// Name of designated contact
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Name of designated contact")]
    public string? Name { get; set; }


    /// <summary>
    /// Telephone number of designated contact
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Telephone number of designated contact")]
    public string? Telephone { get; set; }


    /// <summary>
    /// Email address of designated contact
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Email address of designated contact")]
    public string? Email { get; set; }


    /// <summary>
    /// Name of agent representing designated contact
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Name of agent representing designated contact")]
    public string? Agent { get; set; }

}
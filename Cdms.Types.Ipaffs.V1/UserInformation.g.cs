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


namespace Cdms.Types.Ipaffs;

/// <summary>
/// Information about logged-in user
/// </summary>
public partial class UserInformation  //
{


        /// <summary>
        /// Display name
        /// </summary>
    [JsonPropertyName("displayName")]
    [Cdms.SensitiveData.SensitiveData]
    public string? DisplayName { get; set; }

	
        /// <summary>
        /// User ID
        /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

	
        /// <summary>
        /// Is this user a control
        /// </summary>
    [JsonPropertyName("isControlUser")]
    public bool? IsControlUser { get; set; }

	}



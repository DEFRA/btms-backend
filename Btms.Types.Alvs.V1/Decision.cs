
#nullable enable

using System.Text.Json.Serialization;
using System.Dynamic;


namespace Btms.Types.Alvs;

/// <summary>
/// This is a copy of the AlvsClearanceRequest
/// As a temporary measure to allow us to distinguish between the two types when
/// selecting consumers via the DI container. We'll also start to
/// Plan is to follow the same approach as other types (currently generated) in time.
/// </summary>
public class Decision //
{


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("serviceHeader")]
    public ServiceHeader? ServiceHeader { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("header")]
    public Header? Header { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("items")]
    public Items[]? Items { get; set; }

}
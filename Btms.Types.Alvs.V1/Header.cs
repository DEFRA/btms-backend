using System.Text.Json.Serialization;

namespace Btms.Types.Alvs;

public partial class Header
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("decisionNumber")]
    public int? DecisionNumber { get; set; }
}
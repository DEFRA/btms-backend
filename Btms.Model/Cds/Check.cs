using System.Text.Json.Serialization;

namespace Btms.Model.Cds;

public partial class Check
{
    [JsonPropertyName("decisionInternalFurtherDetail")]
    public string[]? DecisionInternalFurtherDetail { get; set; }
}

#nullable enable

using System.Text.Json.Serialization;
using System.Dynamic;


namespace Btms.Types.Alvs;

public class Finalisation
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("serviceHeader")]
    public required ServiceHeader ServiceHeader { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("header")]
    public required FinalisationHeader Header { get; set; }
}

public partial class FinalisationHeader
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("entryReference")]
    public required string EntryReference { get; set; }
	
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("entryVersionNumber")]
    public required int EntryVersionNumber { get; set; }
    
    /// <summary>
    /// 
    /// </summary
    [JsonPropertyName("decisionNumber")]
    public int? DecisionNumber { get; set; }
    
    /// <summary>
    /// 
    /// </summary
    [JsonPropertyName("finalState")]
    public required string FinalState { get; set; }
    
    /// <summary>
    /// 
    /// </summary
    [JsonPropertyName("manualAction")]
    public required string ManualAction { get; set; }
}
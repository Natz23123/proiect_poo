using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace proiect_poo
{
    public class BlockJsonDefinition
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("text")] public string Text { get; set; }
        [JsonPropertyName("decisionsRequired")] public int DecisionsRequired { get; set; } = 0;
        [JsonPropertyName("decisions")] public List<DecisionJsonDefinition> Decisions { get; set; } = new List<DecisionJsonDefinition>();
        [JsonPropertyName("blockType")] public string BlockType { get; set; } = "normal";
        [JsonPropertyName("nextBlock")] public string NextBlock { get; set; }
    }
}
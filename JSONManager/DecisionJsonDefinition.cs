using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace proiect_poo
{
    public class DecisionJsonDefinition
    {
        [JsonPropertyName("text")] public string Text { get; set; }
        [JsonPropertyName("targetBlock")] public string TargetBlock { get; set; }
        [JsonPropertyName("effects")] public List<EffectJsonDefinition> Effects { get; set; } = new List<EffectJsonDefinition>();
        [JsonPropertyName("condition")] public ConditionNode Condition { get; set; }
        [JsonPropertyName("unlocksIdeaId")] public string UnlocksIdeaId { get; set; }
        [JsonPropertyName("icon")] public string Icon { get; set; }
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace proiect_poo
{
    public class IdeaJsonDefinition
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("researchLevels")] public List<ResearchLevelJsonDefinition> ResearchLevels { get; set; } = new List<ResearchLevelJsonDefinition>();
    }

    public class ResearchLevelJsonDefinition
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("effects")]
        public List<EffectJsonDefinition> Effects { get; set; } = new List<EffectJsonDefinition>();
    }

}
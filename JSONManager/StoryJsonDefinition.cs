using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace proiect_poo
{
    public class StoryJsonDefinition
    {
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("startBlock")] public string StartBlock { get; set; }
        [JsonPropertyName("properties")] public List<PropertyJsonDefinition> Properties { get; set; } = new List<PropertyJsonDefinition>();
        [JsonPropertyName("days")] public List<DayJsonDefinition> Days { get; set; } = new List<DayJsonDefinition>();
    }
}
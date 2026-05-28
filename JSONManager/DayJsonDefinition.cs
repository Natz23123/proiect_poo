using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace proiect_poo
{
    public class DayJsonDefinition
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("blocks")] public List<BlockJsonDefinition> Blocks { get; set; } = new List<BlockJsonDefinition>();
    }
}
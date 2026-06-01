using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace proiect_poo {
    public class PropertyJsonDefinition
    {
        [JsonPropertyName("key")] public string Key { get; set; }
        [JsonPropertyName("hudLabel")] public string HudLabel { get; set; }
        [JsonPropertyName("min")] public int Min { get; set; }
        [JsonPropertyName("max")] public int Max { get; set; }
        [JsonPropertyName("initial")] public int Initial { get; set; }
        [JsonPropertyName("visibleInHud")] public bool VisibleInHud { get; set; }
        [JsonPropertyName("hudOrder")] public int HudOrder { get; set; }
        [JsonPropertyName("onMinBlock")] public string OnMinBlock { get; set; }
        [JsonPropertyName("onMaxBlock")] public string OnMaxBlock { get; set; }
        [JsonPropertyName("icon")] public string Icon { get; set; }
    }
}

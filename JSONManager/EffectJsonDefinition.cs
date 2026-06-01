using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace proiect_poo
{
    public class EffectJsonDefinition
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "ADD";
        [JsonPropertyName("property")] public string Property { get; set; }
        [JsonPropertyName("value")] public int Value { get; set; }
        public override string ToString()
        {
            return $"{Type} {Property} {Value:+#;-#}";
        }

    }

}

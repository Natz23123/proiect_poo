using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace proiect_poo
{
    // =========================================================================
    // 1. CLASA DE BAZĂ ABSTRACTĂ (Aici era lipsa din proiectul tău)
    // =========================================================================
    [JsonConverter(typeof(ConditionNodeConverter))]
    public abstract class ConditionNode
    {
        // Fiecare tip de condiție va trebui să implementeze această metodă
        // pentru a întoarce TRUE (buton activ) sau FALSE (buton ascuns/blocat)
        public abstract bool Evaluate(List<Status> statusuri);
    }

    // =========================================================================
    // 2. NOD PENTRU COMPARAȚII MATEMATICE (ex: "player.innovation >= 20")
    // =========================================================================
    public class ComparisonNode : ConditionNode
    {
        public string Property { get; set; }
        public string Operator { get; set; }
        public int Value { get; set; }

        public override bool Evaluate(List<Status> statusuri)
        {
            // Căutăm statusul curent din joc care corespunde cheii din JSON
            var status = statusuri.FirstOrDefault(s => s.Key == Property);
            if (status == null) return false; // Dacă proprietatea nu există în joc, condiția pică

            // Evaluăm operatorul matematic specificat în JSON
            switch (Operator)
            {
                case "==": return status.Valoare == Value;
                case "!=": return status.Valoare != Value;
                case ">": return status.Valoare > Value;
                case ">=": return status.Valoare >= Value;
                case "<": return status.Valoare < Value;
                case "<=": return status.Valoare <= Value;
                default: return false;
            }
        }
    }

    // =========================================================================
    // 3. NOD PENTRU OPERATORI LOGICI (AND / OR - pentru a lega mai multe condiții)
    // =========================================================================
    public class LogicalNode : ConditionNode
    {
        public string Operator { get; set; } // Poate fi "AND" sau "OR"
        public List<ConditionNode> Children { get; set; } = new List<ConditionNode>();

        public override bool Evaluate(List<Status> statusuri)
        {
            if (Children == null || Children.Count == 0) return true;

            if (Operator == "AND")
            {
                // Toate condițiile din listă trebuie să fie adevărate
                return Children.All(child => child.Evaluate(statusuri));
            }
            if (Operator == "OR")
            {
                // Cel puțin una dintre condiții trebuie să fie adevărată
                return Children.Any(child => child.Evaluate(statusuri));
            }

            return false;
        }
    }

    // =========================================================================
    // 4. CONVERTER CUSTOM PENTRU DESERIALIZARE DIN JSON
    // Acest bloc ajută biblioteca System.Text.Json să știe ce obiect dinamic
    // să creeze în memorie (ComparisonNode sau LogicalNode) când citește fișierul.
    // =========================================================================
    public class ConditionNodeConverter : JsonConverter<ConditionNode>
    {
        public override ConditionNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeProp)) return null;

                string type = typeProp.GetString();

                if (type == "COMPARISON")
                {
                    return new ComparisonNode
                    {
                        Property = root.GetProperty("property").GetString(),
                        Operator = root.GetProperty("operator").GetString(),
                        Value = root.GetProperty("value").GetInt32()
                    };
                }
                else if (type == "LOGICAL")
                {
                    var node = new LogicalNode
                    {
                        Operator = root.GetProperty("operator").GetString(),
                        Children = new List<ConditionNode>()
                    };

                    if (root.TryGetProperty("children", out var childrenProp))
                    {
                        foreach (var childElement in childrenProp.EnumerateArray())
                        {
                            var childNode = JsonSerializer.Deserialize<ConditionNode>(childElement.GetRawText(), options);
                            if (childNode != null) node.Children.Add(childNode);
                        }
                    }
                    return node;
                }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, ConditionNode value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, options);
        }
    }
}
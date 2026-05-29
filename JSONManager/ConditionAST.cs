using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using NJson = Newtonsoft.Json;
using NJsonLinq = Newtonsoft.Json.Linq;
using TJson = System.Text.Json;
using TJsonSer = System.Text.Json.Serialization;

namespace proiect_poo
{
    // Am lăsat doar cele două atribute corecte și explicite
    [TJsonSer.JsonConverter(typeof(ConditionNodeSystemTextConverter))]
    [NJson.JsonConverter(typeof(ConditionNodeNewtonsoftConverter))]
    public abstract class ConditionNode
    {
        public abstract bool Evaluate(List<Status> statusuri);
    }

    public class ComparisonNode : ConditionNode
    {
        public string Property { get; set; }
        public string Operator { get; set; }
        public int Value { get; set; }

        public override bool Evaluate(List<Status> statusuri)
        {
            if (statusuri == null) return false;
            var status = statusuri.FirstOrDefault(s => s.Key.Equals(Property, StringComparison.OrdinalIgnoreCase));
            if (status == null) return false;

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

    public class LogicalNode : ConditionNode
    {
        public string Operator { get; set; } // "AND" sau "OR"
        public List<ConditionNode> Children { get; set; } = new List<ConditionNode>();

        public override bool Evaluate(List<Status> statusuri)
        {
            if (Children == null || Children.Count == 0) return true;

            string op = Operator?.ToUpper();

            if (op == "AND")
            {
                return Children.All(child => child.Evaluate(statusuri));
            }
            if (op == "OR")
            {
                return Children.Any(child => child.Evaluate(statusuri));
            }

            return false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. CONVERTER PENTRU SYSTEM.TEXT.JSON (Redenumit pentru a evita conflictele)
    // ─────────────────────────────────────────────────────────────────────────
    public class ConditionNodeSystemTextConverter : TJsonSer.JsonConverter<ConditionNode>
    {
        public override ConditionNode Read(ref TJson.Utf8JsonReader reader, Type typeToConvert, TJson.JsonSerializerOptions options)
        {
            using (TJson.JsonDocument doc = TJson.JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                // Verificăm dacă există proprietatea Type explicit sau dacă structura are copii (specific LogicalNode)
                bool areType = root.TryGetProperty("type", out var typeProp) || root.TryGetProperty("Type", out typeProp);
                string type = areType ? typeProp.GetString()?.ToUpper() : null;

                bool areCopii = root.TryGetProperty("children", out var cProp) || root.TryGetProperty("Children", out cProp);

                if (type == "LOGICAL" || type == "AND" || type == "OR" || areCopii)
                {
                    string op = root.TryGetProperty("operator", out var o) || root.TryGetProperty("Operator", out o) ? o.GetString() : "AND";
                    var node = new LogicalNode { Operator = op, Children = new List<ConditionNode>() };

                    if (areCopii && cProp.ValueKind == TJson.JsonValueKind.Array)
                    {
                        foreach (var childElement in cProp.EnumerateArray())
                        {
                            var childNode = TJson.JsonSerializer.Deserialize<ConditionNode>(childElement.GetRawText(), options);
                            if (childNode != null) node.Children.Add(childNode);
                        }
                    }
                    return node;
                }
                else
                {
                    // Structură implicită de tip Comparison (cum e în default_story.json-ul tău)
                    string prop = root.TryGetProperty("property", out var p) || root.TryGetProperty("Property", out p) ? p.GetString() : "";
                    string op = root.TryGetProperty("operator", out var o) || root.TryGetProperty("Operator", out o) ? o.GetString() : "";
                    int val = root.TryGetProperty("value", out var v) || root.TryGetProperty("Value", out v) ? v.GetInt32() : 0;

                    return new ComparisonNode { Property = prop, Operator = op, Value = val };
                }
            }
        }

        public override void Write(TJson.Utf8JsonWriter writer, ConditionNode value, TJson.JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value is ComparisonNode comp)
            {
                writer.WriteString("type", "COMPARISON");
                writer.WriteString("Property", comp.Property);
                writer.WriteString("Operator", comp.Operator);
                writer.WriteNumber("Value", comp.Value);
            }
            else if (value is LogicalNode log)
            {
                writer.WriteString("type", "LOGICAL");
                writer.WriteString("Operator", log.Operator);
                writer.WritePropertyName("Children");
                writer.WriteStartArray();
                foreach (var child in log.Children)
                {
                    TJson.JsonSerializer.Serialize(writer, child, options);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. CONVERTER PENTRU NEWTONSOFT.JSON (Reparat pentru compatibilitate cu ambele formate JSON)
    // ─────────────────────────────────────────────────────────────────────────
    public class ConditionNodeNewtonsoftConverter : NJson.JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(ConditionNode).IsAssignableFrom(objectType);

        public override object ReadJson(NJson.JsonReader reader, Type objectType, object existingValue, NJson.JsonSerializer serializer)
        {
            if (reader.TokenType == NJson.JsonToken.Null) return null;

            var jo = NJsonLinq.JObject.Load(reader);
            var typeToken = jo["type"] ?? jo["Type"];
            string type = typeToken?.Value<string>()?.ToUpper();

            // Dacă are câmpul Children sau tipul este Logical, e nod logic (AND/OR)
            if (type == "LOGICAL" || jo["children"] != null || jo["Children"] != null)
            {
                var node = new LogicalNode();
                node.Operator = (jo["operator"] ?? jo["Operator"])?.Value<string>() ?? "AND";

                var childrenToken = jo["children"] ?? jo["Children"];
                if (childrenToken != null && childrenToken.Type == NJsonLinq.JTokenType.Array)
                {
                    node.Children = childrenToken.ToObject<List<ConditionNode>>(serializer);
                }
                return node;
            }
            else
            {
                // Fallback inteligent pentru JSON-uri simple (fără câmpul explicit "type")
                var node = new ComparisonNode();
                node.Property = (jo["property"] ?? jo["Property"])?.Value<string>();
                node.Operator = (jo["operator"] ?? jo["Operator"])?.Value<string>();
                node.Value = (jo["value"] ?? jo["Value"])?.Value<int>() ?? 0;
                return node;
            }
        }

        public override void WriteJson(NJson.JsonWriter writer, object value, NJson.JsonSerializer serializer)
        {
            var jo = new NJsonLinq.JObject();
            if (value is ComparisonNode comp)
            {
                jo["type"] = "COMPARISON";
                jo["Property"] = comp.Property;
                jo["Operator"] = comp.Operator;
                jo["Value"] = comp.Value;
            }
            else if (value is LogicalNode log)
            {
                jo["type"] = "LOGICAL";
                jo["Operator"] = log.Operator;
                jo["Children"] = NJsonLinq.JArray.FromObject(log.Children, serializer);
            }
            jo.WriteTo(writer);
        }
    }
}
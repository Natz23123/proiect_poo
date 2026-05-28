using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace proiect_poo
{
    public class JsonManager
    {
        public static StoryJsonDefinition IncarcaPoveste(string caleFisier)
        {
            if (!File.Exists(caleFisier))
            {
                throw new FileNotFoundException($"Fișierul '{caleFisier}' nu a fost găsit.");
            }

            string jsonString = File.ReadAllText(caleFisier, Encoding.UTF8);
            return JsonSerializer.Deserialize<StoryJsonDefinition>(jsonString);
        }

        public static void SalveazaPoveste(string caleFisier, StoryJsonDefinition poveste)
        {
            var optiuni = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string jsonString = JsonSerializer.Serialize(poveste, optiuni);

            File.WriteAllText(caleFisier, jsonString, Encoding.UTF8);
        }
    }
}
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonTranslate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Utility to translate json");

            // Get json file path
            Console.Write("Json file: ");
            var jsonFilePath = Console.ReadLine();

            // Get language
            Console.Write("Language: ");
            var languageCode = Console.ReadLine();
            
            GenerateTranslatedJson(jsonFilePath, languageCode);
        }

        /// <summary>
        /// Generates a new translated json file
        /// </summary>
        private static void GenerateTranslatedJson(string jsonFilePath, string languageCode)
        {
            JObject originalJson = JObject.Parse(File.ReadAllText(jsonFilePath));
            JObject translatedJson = TranslateJson(originalJson, languageCode);
            var directory = Path.GetDirectoryName(jsonFilePath);
            File.WriteAllText($"{directory}/{languageCode}.json", JsonConvert.SerializeObject(translatedJson, Formatting.Indented));
        }

        private static JObject TranslateJson(JObject originalJson, string languageCode)
        {
            if(originalJson == null || !originalJson.HasValues)
                return originalJson;

            foreach(var keyValue in originalJson)
            {
                var token = keyValue.Value;
                switch(token.Type)
                {
                    case JTokenType.String:
                        if(token is JProperty jproperty) 
                        {
                            jproperty.Value = GetTranslation(keyValue.Key, (string)jproperty.Value, languageCode);
                        }
                        else if(token is JValue jvalue)
                        {
                            jvalue.Value = GetTranslation(keyValue.Key, (string)jvalue.Value, languageCode);
                        }
                        break;
                    case JTokenType.Object:
                        TranslateJson(token as JObject, languageCode);
                        break;
                    case JTokenType.Array:
                        // Converting to list as we have to use for loop instead of foreach
                        // Avoiding foreach as we can't iterate and update (JToken.Replace) the collection at the same time
                        var allArrayItems = token.Values<JToken>().ToList();
                        var count = allArrayItems.Count;
                        for(int ctr = 0; ctr < count; ctr++)
                        {
                            var newToken = TranslateJson(allArrayItems[ctr] as JObject, languageCode);
                            allArrayItems[ctr].Replace(newToken);
                        }
                        break;
                }
            }
            return originalJson;
        }

        private static string GetTranslation(string key, string defaultText, string languageCode)
        {
            if (long.TryParse(defaultText, out _)
                || double.TryParse(defaultText, out _))
                return defaultText;
            return "xxx";
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace OrangeJetpack.Localization
{
    public class LocalizedContent
    {
        /// <summary>
        /// The default language to use when a language is needed.
        /// </summary>
        /// <returns>The first language in AppSettings["LocalizationLanguages"] or "en" if no settings.</returns>
        public static string DefaultLanguage => GetLanguagesFromAppSettings().First();

        private static IEnumerable<string> GetLanguagesFromAppSettings()
        {
            var appSetting = ConfigurationManager.AppSettings["LocalizationLanguages"];
            if (string.IsNullOrWhiteSpace(appSetting))
            {
                return new[]{ "en" };
            }

            return appSetting.Split(',');
        }

        /// <summary>
        /// The key property for a localized field, represents the field's language code.
        /// </summary>
        [JsonProperty(PropertyName = "k")]
        public string Key { get; set; }

        /// <summary>
        /// The value property for a localized field, represent the field's text value.
        /// </summary>
        [JsonProperty(PropertyName = "v")]
        public string Value { get; set; }

        public LocalizedContent()
        {

        }

        public LocalizedContent(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Initializes a new serialized collection of LocalizedContent items using default languages.
        /// </summary>
        public static string Init()
        {
            var defaults = GetLanguagesFromAppSettings().Select(i => new LocalizedContent(i, ""));

            return Serialize(defaults);
        }

        /// <summary>
        /// Serializes a collection of LocalizedContent items to a JSON string.
        /// </summary>
        /// <param name="localizedContents">A collection of LocalizedContent items for serialization.</param>
        public static string Serialize(IEnumerable<LocalizedContent> localizedContents)
        {
            return JsonConvert.SerializeObject(localizedContents);
        }

        /// <summary>
        /// Deserializes a serialized collection of LocalizedContent items.
        /// </summary>
        /// <param name="serializedContents">A JSON serialized collection of LocalizedContent items.</param>
        public static LocalizedContent[] Deserialize(string serializedContents)
        {
            return JsonConvert.DeserializeObject<LocalizedContent[]>(serializedContents);
        }

        /// <summary>
        /// Deserializes a serialized collection of LocalizedContent items, returns a new collection with default language if JSON reader exception occurs.
        /// </summary>
        /// <param name="serializedContents">A JSON serialized collection of LocalizedContent items.</param>
        /// <param name="localizedContent">The output deserialized collection of LocalizedContentItems.</param>
        public static bool TryDeserialize(string serializedContents, out LocalizedContent[] localizedContent)
        {
            try
            {
                localizedContent = Deserialize(serializedContents);
                return true;
            }
            catch (Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException)
            {
                localizedContent = new[] { new LocalizedContent(DefaultLanguage, serializedContents) };
                return false;
            }
        }
    }
}

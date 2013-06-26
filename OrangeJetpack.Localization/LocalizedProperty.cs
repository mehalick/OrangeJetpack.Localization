using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using OrangeJetpack.Localization.Interfaces;

namespace OrangeJetpack.Localization
{
    public class LocalizedProperty
    {
        public const string DefaultLanguage = "en";

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

        public LocalizedProperty()
        {

        }

        public LocalizedProperty(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public static string Init()
        {
            var defaults = new[]
            {
                new LocalizedProperty("en", ""),
                new LocalizedProperty("ar", "")
            };

            return JsonConvert.SerializeObject(defaults);
        }

        public static LocalizedProperty[] Deserialize(string serializedFields)
        {
            return JsonConvert.DeserializeObject<LocalizedProperty[]>(serializedFields);
        }
    }

    public static class LocalizedPropertyExtenions
    {
        public static string Serialize(this LocalizedProperty property)
        {
            var properties = new[] { property };

            return JsonConvert.SerializeObject(properties);
        }

        public static string Serialize(this LocalizedProperty[] properties)
        {
            return JsonConvert.SerializeObject(properties);
        }

        /// <summary>
        /// Returns an item with one or more localized properties deserialized and set to specified language. 
        /// </summary>
        /// <param name="item">The items to localize.</param>
        /// <param name="language">The language to use for localization.</param>
        /// <param name="properties">The collection of localized properties to process.</param>
        /// <remarks>If the specified language is not found the default language or first in list is used.</remarks>
        public static T Localize<T>(this T item, string language, params Expression<Func<T, string>>[] properties) where T : class, ILocalizedEntity
        {
            if (item == null)
            {
                return null;
            }

            LocalizeProperties(item, language, properties);

            return item;
        }

        /// <summary>
        /// Returns a collection with one or more localized properties deserialized and set to specified language. 
        /// </summary>
        /// <param name="items">The collection of items to localize.</param>
        /// <param name="language">The language to use for localization.</param>
        /// <param name="properties">The collection of localized properties to process.</param>
        /// <remarks>If the specified language is not found the default language or first in list is used.</remarks>
        public static IEnumerable<T> Localize<T>(this IEnumerable<T> items, string language, params Expression<Func<T, string>>[] properties) where T : class, ILocalizedEntity
        {
            foreach (var item in items)
            {
                LocalizeProperties(item, language, properties);

                yield return item;
            }
        }

        private static void LocalizeProperties<T>(T item, string language, IEnumerable<Expression<Func<T, string>>> properties) where T : class, ILocalizedEntity
        {
            foreach (var property in properties)
            {
                var memberExpression = (MemberExpression)property.Body;
                var propertyValue = property.Compile()(item);
                if (string.IsNullOrEmpty(propertyValue))
                {
                    continue;
                }

                var localizedFields = LocalizedProperty.Deserialize(propertyValue);
                var fieldForLanguage = GetPropertyForLanguage(localizedFields, language);

                var propertyInfo = (PropertyInfo)memberExpression.Member;
                propertyInfo.SetValue(item, fieldForLanguage.Value, null);
            }
        }

        private static LocalizedProperty GetPropertyForLanguage(LocalizedProperty[] localizedProperties, string language)
        {
            if (!localizedProperties.Any())
            {
                throw new ArgumentException("Cannot localize property, no localized property values exist.", "localizedProperties");
            }

            var localizedProperty = localizedProperties.SingleOrDefault(i => i.Key.Equals(language));
            if (localizedProperty == null && language != LocalizedProperty.DefaultLanguage)
            {
                localizedProperty = GetPropertyForDefaultLanguageOrFirst(localizedProperties);
            }
            return localizedProperty;
        }

        private static LocalizedProperty GetPropertyForDefaultLanguageOrFirst(LocalizedProperty[] localizedProperties)
        {
            return localizedProperties.SingleOrDefault(i => i.Key.Equals(LocalizedProperty.DefaultLanguage)) ??
                   localizedProperties.First();

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OrangeJetpack.Localization
{
    public static class LocalizedContentExtenions
    {
        /// <summary>
        /// Serializes a single LocalizedContent item to a JSON string.
        /// </summary>
        public static string Serialize(this LocalizedContent content)
        {
            var contents = new[] { content };

            return LocalizedContent.Serialize(contents);
        }

        /// <summary>
        /// Serializes a collection of LocalizedContent items to a JSON string.
        /// </summary>
        public static string Serialize(this LocalizedContent[] contents)
        {
            return LocalizedContent.Serialize(contents);
        }

        /// <summary>
        /// Returns an item with one or more localized properties deserialized and set to specified language. 
        /// </summary>
        /// <param name="item">The items to localize.</param>
        /// <param name="language">The language to use for localization.</param>
        /// <param name="properties">The collection of localized properties to process.</param>
        /// <remarks>If the specified language is not found the default language or first in list is used.</remarks>
        public static T Localize<T>(this T item, string language, params Expression<Func<T, string>>[] properties) where T : class, ILocalizable
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
        public static IEnumerable<T> Localize<T>(this IEnumerable<T> items, string language, params Expression<Func<T, string>>[] properties) where T : class, ILocalizable
        {
            foreach (var item in items)
            {
                LocalizeProperties(item, language, properties);

                yield return item;
            }
        }

        private static void LocalizeProperties<T>(T item, string language, IEnumerable<Expression<Func<T, string>>> properties) where T : class, ILocalizable
        {
            foreach (var property in properties)
            {
                var memberExpression = (MemberExpression)property.Body;
                var propertyValue = property.Compile()(item);
                if (string.IsNullOrEmpty(propertyValue))
                {
                    continue;
                }

                var localizedFields = LocalizedContent.Deserialize(propertyValue);
                var fieldForLanguage = GetPropertyForLanguage(localizedFields, language);

                var propertyInfo = (PropertyInfo)memberExpression.Member;
                propertyInfo.SetValue(item, fieldForLanguage.Value, null);
            }
        }

        private static LocalizedContent GetPropertyForLanguage(LocalizedContent[] localizedContents, string language)
        {
            if (!localizedContents.Any())
            {
                throw new ArgumentException("Cannot localize property, no localized property values exist.", "localizedContents");
            }

            var localizedProperty = localizedContents.SingleOrDefault(i => i.Key.Equals(language));
            if (localizedProperty == null && language != LocalizedContent.DefaultLanguage)
            {
                localizedProperty = GetPropertyForDefaultLanguageOrFirst(localizedContents);
            }
            return localizedProperty;
        }

        private static LocalizedContent GetPropertyForDefaultLanguageOrFirst(LocalizedContent[] localizedContents)
        {
            return localizedContents.SingleOrDefault(i => i.Key.Equals(LocalizedContent.DefaultLanguage)) ??
                   localizedContents.First();

        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OrangeJetpack.Localization
{
    public static class LocalizedContentExtensions
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
        public static string Serialize(this IEnumerable<LocalizedContent> contents)
        {
            return LocalizedContent.Serialize(contents);
        }

        /// <summary>
        /// Returns an item with one or more localized properties deserialized and set to specified language. 
        /// </summary>
        /// <param name="item">The items to localize.</param>
        /// <param name="language">The language to use for localization.</param>
        /// <param name="depth">The depth to which child properties should be localized.</param>
        /// <remarks>If the specified language is not found the default language or first in list is used.</remarks>
        public static T Localize<T>(this T item, string language, LocalizationDepth depth = LocalizationDepth.Shallow) where T : class, ILocalizable
        {
            if (item == null)
            {
                return null;
            }

            LocalizeProperties(item, language);
            LocalizeChildren(item, language, depth);
            LocalizeCollections(item, language, depth);

            return item;
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
        /// <param name="depth">The depth to which child properties should be localized.</param>
        /// <remarks>If the specified language is not found the default language or first in list is used.</remarks>
        public static IEnumerable<T> Localize<T>(this IEnumerable<T> items, string language, LocalizationDepth depth = LocalizationDepth.Shallow) where T : class, ILocalizable
        {
            foreach (var item in items)
            {
                LocalizeProperties(item, language);
                LocalizeChildren(item, language, depth);
                LocalizeCollections(item, language, depth);

                yield return item;
            }
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

        private static void LocalizeChildren<T>(T item, string language, LocalizationDepth depth) where T : class, ILocalizable
        {
            if (depth == LocalizationDepth.Shallow)
            {
                return;
            }

            if (depth == LocalizationDepth.OneLevel)
            {
                depth = LocalizationDepth.Shallow;
            }

            var children = item.GetType()
                .GetTypeInfo()
                .GetProperties()
                .Select(i => i.GetValue(item, null) as ILocalizable)
                .Where(i => i != null);

            foreach (var child in children)
            {
                child.Localize(language, depth);
            }
        }

        private static void LocalizeCollections<T>(T item, string language, LocalizationDepth depth) where T : class, ILocalizable
        {
            if (depth == LocalizationDepth.Shallow)
            {
                return;
            }

            if (depth == LocalizationDepth.OneLevel)
            {
                depth = LocalizationDepth.Shallow;
            }

            var collections = item.GetType()
                .GetTypeInfo()
                .GetProperties()
                .Select(i => i.GetValue(item) as IEnumerable)
                .Where(i => i != null);

            foreach (var collection in collections)
            {
                foreach (var element in collection)
                {
                    Localize(element as ILocalizable, language, depth);
                }
            }
        }

        private static void LocalizeProperties<T>(T item, string language)
        {
            var properties = item
                .GetType()
                .GetTypeInfo()
                .GetProperties()
                .Where(i => i.IsDefined(typeof(LocalizedAttribute)));

            foreach (var propertyInfo in properties)
            {
                var propertyValue = propertyInfo.GetValue(item)?.ToString();
                if (string.IsNullOrEmpty(propertyValue))
                {
                    continue;
                }

                LocalizedContent[] localizedContents;
                if (!LocalizedContent.TryDeserialize(propertyValue, out localizedContents))
                {
                    continue;
                }

                var contentForLanguage = GetContentForLanguage(localizedContents, language);
                propertyInfo.SetValue(item, contentForLanguage.Value, null);
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

                LocalizedContent[] localizedContents;
                if (!LocalizedContent.TryDeserialize(propertyValue, out localizedContents))
                {
                    continue;
                }

                var contentForLanguage = GetContentForLanguage(localizedContents, language);

                var propertyInfo = (PropertyInfo)memberExpression.Member;
                propertyInfo.SetValue(item, contentForLanguage.Value, null);
            }
        }

        private static LocalizedContent GetContentForLanguage(LocalizedContent[] localizedContents, string language)
        {
            if (!localizedContents.Any())
            {
                throw new ArgumentException("Cannot localize property, no localized property values exist.", nameof(localizedContents));
            }

            var localizedContent = localizedContents.SingleOrDefault(i => i.Key.Equals(language) && !string.IsNullOrWhiteSpace(i.Value));

            return localizedContent ?? GetContentForDefaultLanguageOrFirst(localizedContents);
        }

        private static LocalizedContent GetContentForDefaultLanguageOrFirst(LocalizedContent[] localizedContents)
        {
            return localizedContents.SingleOrDefault(i => i.Key.Equals(LocalizedContent.DefaultLanguage)) ??
                   localizedContents.First();
        }

        /// <summary>
        /// Sets an item's localized content property from a <see cref="LocalizedContent"/> collection.
        /// </summary>
        /// <param name="item">An ILocalizable item.</param>
        /// <param name="property">The property to set.</param>
        /// <param name="content">The collection of localized content.</param>
        /// <returns>The original item with localized property sety.</returns>
        public static T Set<T>(this T item, Expression<Func<T, string>> property, IEnumerable<LocalizedContent> content) where T : class, ILocalizable
        {
            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            propertyInfo.SetValue(item, content.Serialize(), null);

            return item;
        }

        /// <summary>
        /// Sets an item's localized content property from a dictionary.
        /// </summary>
        /// <param name="item">An ILocalizable item.</param>
        /// <param name="property">The property to set.</param>
        /// <param name="content">The dictionary of localized content.</param>
        /// <returns>The original item with localized property sety.</returns>
        public static T Set<T>(this T item, Expression<Func<T, string>> property, IDictionary<string, string> content) where T : class, ILocalizable
        {
            var localizedContents = content.Select(i => new LocalizedContent
            {
                Key = i.Key,
                Value = i.Value
            });

            return Set(item, property, localizedContents);
        }
    }
}
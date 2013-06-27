using System.Linq;
using NUnit.Framework;
using OrangeJetpack.Localization.Interfaces;

namespace OrangeJetpack.Localization.Tests
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable PossibleNullReferenceException

    [TestFixture]
    public class LocalizedPropertyTests
    {
        public class TestClass : ILocalizedEntity
        {
            public string PropertyA { get; set; }
            public string PropertyB { get; set; }
        }

        private const string ANY_STRING = "ABC123";
        private readonly string NOT_DEFAULT_LANGUAGE = LocalizedProperty.DefaultLanguage + "zz";
        private const string NOT_DEFAULT_ANY_STRING = ANY_STRING + "zz";

        [Test]
        public void Localize_SinglePropertyWithDefaultLanguage_LocalizesCorrectly()
        {
            var localizedField = new LocalizedProperty(LocalizedProperty.DefaultLanguage, ANY_STRING);

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedField.Serialize()}
            };

            var localized = testClasses.Localize(LocalizedProperty.DefaultLanguage, i => i.PropertyA);

            Assert.AreEqual(ANY_STRING, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_SinglePropertyWithTwoLanguages_DefaultLanguageLocalizesCorrectly()
        {
            var localizedFields = new[]
            {
                new LocalizedProperty(LocalizedProperty.DefaultLanguage, ANY_STRING),
                new LocalizedProperty(NOT_DEFAULT_LANGUAGE, NOT_DEFAULT_ANY_STRING)
            };

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedFields.Serialize()}
            };

            var localized = testClasses.Localize(LocalizedProperty.DefaultLanguage, i => i.PropertyA);

            Assert.AreEqual(ANY_STRING, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_SinglePropertyWithTwoLanguages_NonDefaultLanguageLocalizesCorrectly()
        {
            var localizedFields = new[]
            {
                new LocalizedProperty(LocalizedProperty.DefaultLanguage, ANY_STRING),
                new LocalizedProperty(NOT_DEFAULT_LANGUAGE, NOT_DEFAULT_ANY_STRING)
            };

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedFields.Serialize()}
            };

            var localized = testClasses.Localize(NOT_DEFAULT_LANGUAGE, i => i.PropertyA);

            Assert.AreEqual(NOT_DEFAULT_ANY_STRING, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_MultiplePropertiesWithTwoLanguages_NonDefaultLanguageLocalizesCorrectly()
        {
            var localizedFields = new[]
            {
                new LocalizedProperty(LocalizedProperty.DefaultLanguage, ANY_STRING),
                new LocalizedProperty(NOT_DEFAULT_LANGUAGE, NOT_DEFAULT_ANY_STRING)
            };

            var testClasses = new[]
            {
                new TestClass
                {
                    PropertyA = localizedFields.Serialize(),
                    PropertyB = localizedFields.Serialize()
                }
            };

            var localized = testClasses.Localize(NOT_DEFAULT_LANGUAGE, i => i.PropertyA, i => i.PropertyB).ToList();

            Assert.AreEqual(NOT_DEFAULT_ANY_STRING, localized.Single().PropertyA);
            Assert.AreEqual(NOT_DEFAULT_ANY_STRING, localized.Single().PropertyB);
        }

        [Test]
        public void Localize_SinglePropertyRequestedLanguageNotFound_LocalizesToDefaultLanguage()
        {
            var localizedFields = new[]
            {
                new LocalizedProperty(LocalizedProperty.DefaultLanguage, ANY_STRING)
            };

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedFields.Serialize()}
            };

            var localized = testClasses.Localize(NOT_DEFAULT_LANGUAGE, i => i.PropertyA);

            Assert.AreEqual(ANY_STRING, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_SinglePropertyRequestedLanguageNotFoundAndNoDefaultLanguage_LocalizesToFirst()
        {
            var localizedFields = new[]
            {
                new LocalizedProperty(NOT_DEFAULT_LANGUAGE, NOT_DEFAULT_ANY_STRING)
            };

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedFields.Serialize()}
            };

            var someOtherLanguage = NOT_DEFAULT_LANGUAGE + "yy";
            var localized = testClasses.Localize(someOtherLanguage, i => i.PropertyA);

            Assert.AreEqual(NOT_DEFAULT_ANY_STRING, localized.Single().PropertyA);
        }
    }

    // ReSharper restore InconsistentNaming
    // ReSharper restore PossibleNullReferenceException
}

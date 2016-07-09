using System.Linq;
using NUnit.Framework;

namespace OrangeJetpack.Localization.Tests
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable PossibleNullReferenceException

    [TestFixture]
    public class LocalizedContentTests
    {
        public class TestClass : ILocalizable
        {
            [Localized]
            public string PropertyA { get; set; }

            [Localized]
            public string PropertyB { get; set; }

            public TestClass ChildA { get; set; }
            public TestClass ChildB { get; set; }
        }

        private const string DEFAULT_LANGUAGE = "en";
        private const string OTHER_LANGUAGE = "zz";
        private const string ANY_STRING_1 = "ABC123";
        private const string ANY_STRING_2 = "XYZ789";

        private static string GetLocalizedContent()
        {
            var localizedFields = new[]
            {
                new LocalizedContent(DEFAULT_LANGUAGE, ANY_STRING_1),
                new LocalizedContent(OTHER_LANGUAGE, ANY_STRING_2)
            };
            return localizedFields.Serialize();
        }

        [Test]
        public void Localize_NoPropertiesSpecified_LocalizesAllProperties()
        {
            var localizedContent = GetLocalizedContent();

            var testClass1 = new TestClass
            {
                PropertyA = localizedContent,
                PropertyB = localizedContent
            };

            var testClass2 = new TestClass
            {
                PropertyA = localizedContent,
                PropertyB = localizedContent
            };

            testClass1.Localize(DEFAULT_LANGUAGE);

            Assert.AreEqual(ANY_STRING_1, testClass1.PropertyA);
            Assert.AreEqual(ANY_STRING_1, testClass1.PropertyB);

            testClass2.Localize(OTHER_LANGUAGE);

            Assert.AreEqual(ANY_STRING_2, testClass2.PropertyA);
            Assert.AreEqual(ANY_STRING_2, testClass2.PropertyB);
        }

        [Test]
        public void Localize_SinglePropertyWithDefaultLanguage_LocalizesCorrectly()
        {
            var localizedField = new LocalizedContent(DEFAULT_LANGUAGE, ANY_STRING_1);

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedField.Serialize()}
            };

            var localized = testClasses.Localize(DEFAULT_LANGUAGE, i => i.PropertyA);

            Assert.AreEqual(ANY_STRING_1, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_SinglePropertyWithTwoLanguages_DefaultLanguageLocalizesCorrectly()
        {
            var localizedContent = GetLocalizedContent();

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedContent}
            };

            var localized = testClasses.Localize(DEFAULT_LANGUAGE, i => i.PropertyA);

            Assert.AreEqual(ANY_STRING_1, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_SinglePropertyWithTwoLanguages_NonDefaultLanguageLocalizesCorrectly()
        {
            var localizedContent = GetLocalizedContent();

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedContent}
            };

            var localized = testClasses.Localize(OTHER_LANGUAGE, i => i.PropertyA);

            Assert.AreEqual(ANY_STRING_2, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_MultiplePropertiesWithTwoLanguages_NonDefaultLanguageLocalizesCorrectly()
        {
            var localizedContent = GetLocalizedContent();

            var testClasses = new[]
            {
                new TestClass
                {
                    PropertyA = localizedContent,
                    PropertyB = localizedContent
                }
            };

            var localized = testClasses.Localize(OTHER_LANGUAGE, i => i.PropertyA, i => i.PropertyB).ToList();

            Assert.AreEqual(ANY_STRING_2, localized.Single().PropertyA);
            Assert.AreEqual(ANY_STRING_2, localized.Single().PropertyB);
        }

        [Test]
        public void Localize_SinglePropertyRequestedLanguageNotFound_LocalizesToDefaultLanguage()
        {
            var localizedFields = new[]
            {
                new LocalizedContent(DEFAULT_LANGUAGE, ANY_STRING_1)
            };

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedFields.Serialize()}
            };

            var localized = testClasses.Localize(OTHER_LANGUAGE, i => i.PropertyA);

            Assert.AreEqual(ANY_STRING_1, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_SinglePropertyRequestedLanguageNotFoundAndNoDefaultLanguage_LocalizesToFirst()
        {
            var localizedFields = new[]
            {
                new LocalizedContent(OTHER_LANGUAGE, ANY_STRING_2)
            };

            var testClasses = new[]
            {
                new TestClass {PropertyA = localizedFields.Serialize()}
            };

            const string someOtherLanguage = OTHER_LANGUAGE + "yy";
            var localized = testClasses.Localize(someOtherLanguage, i => i.PropertyA);

            Assert.AreEqual(ANY_STRING_2, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_NonserializedProperty_ReturnsOriginalValueWithoutThrowingException()
        {
            const string notSerializedJson = "1";

            var testClasses = new[]
            {
                new TestClass {PropertyA = notSerializedJson}
            };

            var localized = testClasses.Localize(DEFAULT_LANGUAGE, i => i.PropertyA);

            Assert.AreEqual(notSerializedJson, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_WithLocalizableChildren_LocalizesChildren()
        {
            var localizedContent = GetLocalizedContent();

            var testClass = new TestClass
            {
                ChildA = new TestClass
                {
                    PropertyA = localizedContent,
                    ChildA = new TestClass
                    {
                        PropertyA = localizedContent
                    }
                }
            };

            var localized = testClass.Localize(DEFAULT_LANGUAGE);

            Assert.AreEqual(ANY_STRING_1, localized.ChildA.PropertyA);
            Assert.AreEqual(ANY_STRING_1, localized.ChildA.ChildA.PropertyA);
        }

        [Test]
        public void Localize_LocalizationDepthIsShallow_DoesNotLocalizeChildred()
        {
            var localizedContent = GetLocalizedContent();

            var testClass = new TestClass
            {
                ChildA = new TestClass
                {
                    PropertyA = localizedContent,
                    PropertyB = localizedContent
                }
            };

            var localized = testClass.Localize(DEFAULT_LANGUAGE, LocalizationDepth.Shallow);

            Assert.AreNotEqual(ANY_STRING_1, localized.ChildA.PropertyA);
            Assert.AreNotEqual(ANY_STRING_1, localized.ChildA.PropertyB);
        }

        [Test]
        public void Localize_LocalizationDepthIsOneLevel_OnlyImmediateChildrenLocalized()
        {
            var localizedContent = GetLocalizedContent();

            var testClass = new TestClass
            {
                ChildA = new TestClass
                {
                    PropertyA = localizedContent,
                    ChildA = new TestClass
                    {
                        PropertyA = localizedContent
                    }
                }
            };

            var localized = testClass.Localize(DEFAULT_LANGUAGE, LocalizationDepth.OneLevel);

            Assert.AreEqual(ANY_STRING_1, localized.ChildA.PropertyA);
            Assert.AreNotEqual(ANY_STRING_1, localized.ChildA.ChildA.PropertyA);
        }
    }

    // ReSharper restore InconsistentNaming
    // ReSharper restore PossibleNullReferenceException
}

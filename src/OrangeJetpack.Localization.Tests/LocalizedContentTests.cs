using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace OrangeJetpack.Localization.Tests
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable PossibleNullReferenceException

    [TestFixture]
    public class LocalizedContentTests
    {
        public class TestClassA : ILocalizable
        {
            [Localized]
            public string PropertyA { get; set; }

            [Localized]
            public string PropertyB { get; set; }

            public string PropertyC { get; set; } = "NOT LOCALIZED";

            public TestClassA ChildA { get; set; }
            public TestClassB ChildB { get; set; }

            public ICollection<TestClassA> ChildrenA { get; set; }
            public ICollection<TestClassB> ChildrenB { get; set; }
            public ICollection<object> NonLocalizedCollection { get; set; } = new object[] { "I'M", "NOT", "LOCALIZED" };
        }

        public class TestClassB : ILocalizable
        {
            [Localized]
            public string PropertyA { get; set; }

            [Localized]
            public string PropertyB { get; set; }

            public string PropertyC { get; set; } = "NOT LOCALIZED";
        }

        private const string DEFAULT_LANGUAGE = "en";
        private const string OTHER_LANGUAGE = "zz";
        private const string ANY_STRING_1 = "ABC123";
        private const string ANY_STRING_2 = "XYZ789";

        private static string GetLocalizedContent()
        {
            var localizedContent = new[]
            {
                new LocalizedContent(DEFAULT_LANGUAGE, ANY_STRING_1),
                new LocalizedContent(OTHER_LANGUAGE, ANY_STRING_2)
            };
            return localizedContent.Serialize();
        }

        [Test]
        public void Localize_NoPropertiesSpecified_LocalizesAllProperties()
        {
            var localizedContent = GetLocalizedContent();

            var testClass1 = new TestClassA
            {
                PropertyA = localizedContent,
                PropertyB = localizedContent
            };

            var testClass2 = new TestClassA
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
                new TestClassA {PropertyA = localizedField.Serialize()}
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
                new TestClassA {PropertyA = localizedContent}
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
                new TestClassA {PropertyA = localizedContent}
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
                new TestClassA
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
                new TestClassA {PropertyA = localizedFields.Serialize()}
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
                new TestClassA {PropertyA = localizedFields.Serialize()}
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
                new TestClassA {PropertyA = notSerializedJson}
            };

            var localized = testClasses.Localize(DEFAULT_LANGUAGE, i => i.PropertyA);

            Assert.AreEqual(notSerializedJson, localized.Single().PropertyA);
        }

        [Test]
        public void Localize_WithLocalizableChildren_LocalizesChildren()
        {
            var localizedContent = GetLocalizedContent();

            var testClass = new TestClassA
            {
                ChildA = new TestClassA
                {
                    PropertyA = localizedContent,
                    ChildA = new TestClassA
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

            var testClass = new TestClassA
            {
                ChildA = new TestClassA
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

            var testClass = new TestClassA
            {
                ChildA = new TestClassA
                {
                    PropertyA = localizedContent,
                    ChildA = new TestClassA
                    {
                        PropertyA = localizedContent
                    }
                }
            };

            var localized = testClass.Localize(DEFAULT_LANGUAGE, LocalizationDepth.OneLevel);

            Assert.AreEqual(ANY_STRING_1, localized.ChildA.PropertyA);
            Assert.AreNotEqual(ANY_STRING_1, localized.ChildA.ChildA.PropertyA);
        }

        [Test]
        public void Localize_HasCollectionOfLocalizableChildren_LocalizesCollection()
        {
            var localizedContent = GetLocalizedContent();

            var testClass = new TestClassA
            {
                ChildrenA = new[]
                {
                    new TestClassA
                    {
                        PropertyA = localizedContent
                    },
                    new TestClassA
                    {
                        PropertyA = localizedContent
                    }
                },
                ChildrenB = new[]
                {
                    new TestClassB
                    {
                        PropertyA = localizedContent
                    }
                }
            };

            var localized = testClass.Localize(DEFAULT_LANGUAGE);

            Assert.AreEqual(ANY_STRING_1, localized.ChildrenA.ElementAt(0).PropertyA);
            Assert.AreEqual(ANY_STRING_1, localized.ChildrenA.ElementAt(1).PropertyA);
            Assert.AreEqual(ANY_STRING_1, localized.ChildrenB.ElementAt(0).PropertyA);
        }

        [Test]
        public void Set_ProvideLocalizedContentArray_SetsProperty()
        {
            var testClass = new TestClassA();

            var localizedContent = new[]
            {
                new LocalizedContent(DEFAULT_LANGUAGE, ANY_STRING_1),
                new LocalizedContent(OTHER_LANGUAGE, ANY_STRING_2)
            };

            testClass.Set(i => i.PropertyA, localizedContent);

            var localized = testClass.Localize(DEFAULT_LANGUAGE);

            Assert.AreEqual(ANY_STRING_1, localized.PropertyA);
        }

        [Test]
        public void Set_ProvideLocalizedContentDictionary_SetsProperty()
        {
            var testClass = new TestClassA();

            var localizedContent = new Dictionary<string, string>
            {
                {DEFAULT_LANGUAGE, ANY_STRING_1},
                {OTHER_LANGUAGE, ANY_STRING_2}
            };

            testClass.Set(i => i.PropertyA, localizedContent);

            var localized = testClass.Localize(DEFAULT_LANGUAGE);

            Assert.AreEqual(ANY_STRING_1, localized.PropertyA);
        }
    }

    // ReSharper restore InconsistentNaming
    // ReSharper restore PossibleNullReferenceException
}

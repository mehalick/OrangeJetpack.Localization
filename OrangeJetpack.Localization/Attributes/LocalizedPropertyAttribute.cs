using System.ComponentModel.DataAnnotations;

namespace OrangeJetpack.Localization.Attributes
{
    public class LocalizedPropertyAttribute : DataTypeAttribute
    {
        public LocalizedPropertyAttribute() : base("LocalizedProperty")
        {
        }
    }
}

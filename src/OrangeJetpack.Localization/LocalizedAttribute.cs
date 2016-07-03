using System.ComponentModel.DataAnnotations;

namespace OrangeJetpack.Localization
{
    public class LocalizedAttribute : DataTypeAttribute
    {
        public LocalizedAttribute() : base("LocalizedContent")
        {
        }
    }
}
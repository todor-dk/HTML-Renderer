// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using System.Windows.Markup;
using System.Windows.Media;
using Scientia.HtmlRenderer.Adapters;

namespace Scientia.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF Font family object for core.
    /// </summary>
    internal sealed class FontFamilyAdapter : RFontFamily
    {
        /// <summary>
        /// Default language to get font family name by
        /// </summary>
        private static readonly XmlLanguage XmlLanguage = XmlLanguage.GetLanguage("en-us");

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        private readonly FontFamily _FontFamily;

        /// <summary>
        /// Init.
        /// </summary>
        public FontFamilyAdapter(FontFamily fontFamily)
        {
            this._FontFamily = fontFamily;
        }

        /// <summary>
        /// the underline WPF font family.
        /// </summary>
        public FontFamily FontFamily
        {
            get { return this._FontFamily; }
        }

        public override string Name
        {
            get
            {
                string name = this._FontFamily.FamilyNames[XmlLanguage];
                if (string.IsNullOrEmpty(name))
                {
                    foreach (var familyName in this._FontFamily.FamilyNames)
                    {
                        return familyName.Value;
                    }
                }

                return name;
            }
        }
    }
}
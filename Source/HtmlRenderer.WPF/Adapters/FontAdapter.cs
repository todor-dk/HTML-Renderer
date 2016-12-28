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

using System.Windows.Media;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF Font.
    /// </summary>
    internal sealed class FontAdapter : RFont
    {
        #region Fields and Consts

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        private readonly Typeface _Font;

        /// <summary>
        /// The glyph font for the font
        /// </summary>
        private readonly GlyphTypeface _GlyphTypeface;

        /// <summary>
        /// the size of the font
        /// </summary>
        private readonly double _Size;

        /// <summary>
        /// the vertical offset of the font underline location from the top of the font.
        /// </summary>
        private readonly double _UnderlineOffset = -1;

        /// <summary>
        /// Cached font height.
        /// </summary>
        private readonly double _Height = -1;

        /// <summary>
        /// Cached font whitespace width.
        /// </summary>
        private double WhitespaceWidth = -1;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public FontAdapter(Typeface font, double size)
        {
            this._Font = font;
            this._Size = size;
            this._Height = 96d / 72d * this._Size * this._Font.FontFamily.LineSpacing;
            this._UnderlineOffset = 96d / 72d * this._Size * (this._Font.FontFamily.LineSpacing + font.UnderlinePosition);

            GlyphTypeface typeface;
            if (font.TryGetGlyphTypeface(out typeface))
            {
                this._GlyphTypeface = typeface;
            }
            else
            {
                foreach (var sysTypeface in Fonts.SystemTypefaces)
                {
                    if (sysTypeface.TryGetGlyphTypeface(out typeface))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        public Typeface Font
        {
            get { return this._Font; }
        }

        public GlyphTypeface GlyphTypeface
        {
            get { return this._GlyphTypeface; }
        }

        public override double Size
        {
            get { return this._Size; }
        }

        public override double UnderlineOffset
        {
            get { return this._UnderlineOffset; }
        }

        public override double Height
        {
            get { return this._Height; }
        }

        public override double LeftPadding
        {
            get { return this._Height / 6f; }
        }

        public override double GetWhitespaceWidth(RGraphics graphics)
        {
            if (this.WhitespaceWidth < 0)
            {
                this.WhitespaceWidth = graphics.MeasureString(" ", this).Width;
            }

            return this.WhitespaceWidth;
        }
    }
}
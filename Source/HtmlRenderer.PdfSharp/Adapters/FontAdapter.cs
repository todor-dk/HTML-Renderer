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

using TheArtOfDev.HtmlRenderer.Adapters;
using PdfSharp.Drawing;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for WinForms Font object for core.
    /// </summary>
    internal sealed class FontAdapter : RFont
    {
        #region Fields and Consts

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        private readonly XFont _Font;

        /// <summary>
        /// the vertical offset of the font underline location from the top of the font.
        /// </summary>
        private double _UnderlineOffset = -1;

        /// <summary>
        /// Cached font height.
        /// </summary>
        private double _Height = -1;

        /// <summary>
        /// Cached font whitespace width.
        /// </summary>
        private double WhitespaceWidth = -1;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public FontAdapter(XFont font)
        {
            this._Font = font;
        }

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        public XFont Font
        {
            get { return this._Font; }
        }

        public override double Size
        {
            get { return this._Font.Size; }
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

        /// <summary>
        /// Set font metrics to be cached for the font for future use.
        /// </summary>
        /// <param name="height">the full height of the font</param>
        /// <param name="underlineOffset">the vertical offset of the font underline location from the top of the font.</param>
        internal void SetMetrics(int height, int underlineOffset)
        {
            this._Height = height;
            this._UnderlineOffset = underlineOffset;
        }
    }
}
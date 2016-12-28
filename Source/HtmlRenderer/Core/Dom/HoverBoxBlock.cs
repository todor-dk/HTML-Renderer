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

using TheArtOfDev.HtmlRenderer.Core.Entities;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// CSS boxes that have ":hover" selector on them.
    /// </summary>
    internal sealed class HoverBoxBlock
    {
        /// <summary>
        /// the box that has :hover css on
        /// </summary>
        private readonly CssBox _CssBox;

        /// <summary>
        /// the :hover style block data
        /// </summary>
        private readonly CssBlock _CssBlock;

        /// <summary>
        /// Init.
        /// </summary>
        public HoverBoxBlock(CssBox cssBox, CssBlock cssBlock)
        {
            this._CssBox = cssBox;
            this._CssBlock = cssBlock;
        }

        /// <summary>
        /// the box that has :hover css on
        /// </summary>
        public CssBox CssBox
        {
            get { return this._CssBox; }
        }

        /// <summary>
        /// the :hover style block data
        /// </summary>
        public CssBlock CssBlock
        {
            get { return this._CssBlock; }
        }
    }
}
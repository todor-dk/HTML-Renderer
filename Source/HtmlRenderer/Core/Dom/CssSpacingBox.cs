using System;
using System.Collections.Generic;
using Scientia.HtmlRenderer.Core.Utils;

namespace Scientia.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Used to make space on vertical cell combination
    /// </summary>
    internal sealed class CssSpacingBox : CssBox
    {
        #region Fields and Consts

        private readonly CssBox _ExtendedBox;

        /// <summary>
        /// the index of the row where box starts
        /// </summary>
        private readonly int _StartRow;

        /// <summary>
        /// the index of the row where box ends
        /// </summary>
        private readonly int _EndRow;

        #endregion

        public CssSpacingBox(CssBox tableBox, ref CssBox extendedBox, int startRow)
            : base(tableBox, new HtmlTag("none", false, new Dictionary<string, string> { { "colspan", "1" } }))
        {
            this._ExtendedBox = extendedBox;
            this.Display = CssConstants.None;

            this._StartRow = startRow;
            this._EndRow = startRow + Int32.Parse(extendedBox.GetAttribute("rowspan", "1")) - 1;
        }

        public CssBox ExtendedBox
        {
            get { return this._ExtendedBox; }
        }

        /// <summary>
        /// Gets the index of the row where box starts
        /// </summary>
        public int StartRow
        {
            get { return this._StartRow; }
        }

        /// <summary>
        /// Gets the index of the row where box ends
        /// </summary>
        public int EndRow
        {
            get { return this._EndRow; }
        }
    }
}
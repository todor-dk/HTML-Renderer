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

using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// Invoked when a stylesheet is about to be loaded by file path or URL in 'link' element.<br/>
    /// Allows to overwrite the loaded stylesheet by providing the stylesheet data manually, or different source (file or URL) to load from.<br/>
    /// Example: The stylesheet 'href' can be non-valid URI string that is interpreted in the overwrite delegate by custom logic to pre-loaded stylesheet object<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </summary>
    public sealed class HtmlStylesheetLoadEventArgs : EventArgs
    {
        #region Fields and Consts

        /// <summary>
        /// the source of the stylesheet as found in the HTML (file path or URL)
        /// </summary>
        private readonly string _Src;

        /// <summary>
        /// collection of all the attributes that are defined on the link element
        /// </summary>
        private readonly Dictionary<string, string> _Attributes;

        /// <summary>
        /// provide the new source (file path or URL) to load stylesheet from
        /// </summary>
        private string _SetSrc;

        /// <summary>
        /// provide the stylesheet to load
        /// </summary>
        private string _SetStyleSheet;

        /// <summary>
        /// provide the stylesheet data to load
        /// </summary>
        private CssData _SetStyleSheetData;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="src">the source of the image (file path or URL)</param>
        /// <param name="attributes">collection of all the attributes that are defined on the image element</param>
        internal HtmlStylesheetLoadEventArgs(string src, Dictionary<string, string> attributes)
        {
            this._Src = src;
            this._Attributes = attributes;
        }

        /// <summary>
        /// the source of the stylesheet as found in the HTML (file path or URL)
        /// </summary>
        public string Src
        {
            get { return this._Src; }
        }

        /// <summary>
        /// collection of all the attributes that are defined on the link element
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get { return this._Attributes; }
        }

        /// <summary>
        /// provide the new source (file path or URL) to load stylesheet from
        /// </summary>
        public string SetSrc
        {
            get { return this._SetSrc; }
            set { this._SetSrc = value; }
        }

        /// <summary>
        /// provide the stylesheet to load
        /// </summary>
        public string SetStyleSheet
        {
            get { return this._SetStyleSheet; }
            set { this._SetStyleSheet = value; }
        }

        /// <summary>
        /// provide the stylesheet data to load
        /// </summary>
        public CssData SetStyleSheetData
        {
            get { return this._SetStyleSheetData; }
            set { this._SetStyleSheetData = value; }
        }
    }
}
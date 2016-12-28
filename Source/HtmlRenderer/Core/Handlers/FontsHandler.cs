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
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// Utilities for fonts and fonts families handling.
    /// </summary>
    internal sealed class FontsHandler
    {
        #region Fields and Consts

        /// <summary>
        ///
        /// </summary>
        private readonly RAdapter Adapter;

        /// <summary>
        /// Allow to map not installed fonts to different
        /// </summary>
        private readonly Dictionary<string, string> FontsMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// collection of all installed and added font families to check if font exists
        /// </summary>
        private readonly Dictionary<string, RFontFamily> ExistingFontFamilies = new Dictionary<string, RFontFamily>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// cache of all the font used not to create same font again and again
        /// </summary>
        private readonly Dictionary<string, Dictionary<double, Dictionary<RFontStyle, RFont>>> FontsCache = new Dictionary<string, Dictionary<double, Dictionary<RFontStyle, RFont>>>(StringComparer.InvariantCultureIgnoreCase);

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public FontsHandler(RAdapter adapter)
        {
            ArgChecker.AssertArgNotNull(adapter, "global");

            this.Adapter = adapter;
        }

        /// <summary>
        /// Check if the given font family exists by name
        /// </summary>
        /// <param name="family">the font to check</param>
        /// <returns>true - font exists by given family name, false - otherwise</returns>
        public bool IsFontExists(string family)
        {
            bool exists = this.ExistingFontFamilies.ContainsKey(family);
            if (!exists)
            {
                string mappedFamily;
                if (this.FontsMapping.TryGetValue(family, out mappedFamily))
                {
                    exists = this.ExistingFontFamilies.ContainsKey(mappedFamily);
                }
            }

            return exists;
        }

        /// <summary>
        /// Adds a font family to be used.
        /// </summary>
        /// <param name="fontFamily">The font family to add.</param>
        public void AddFontFamily(RFontFamily fontFamily)
        {
            ArgChecker.AssertArgNotNull(fontFamily, "family");

            this.ExistingFontFamilies[fontFamily.Name] = fontFamily;
        }

        /// <summary>
        /// Adds a font mapping from <paramref name="fromFamily"/> to <paramref name="toFamily"/> iff the <paramref name="fromFamily"/> is not found.<br/>
        /// When the <paramref name="fromFamily"/> font is used in rendered html and is not found in existing
        /// fonts (installed or added) it will be replaced by <paramref name="toFamily"/>.<br/>
        /// </summary>
        /// <param name="fromFamily">the font family to replace</param>
        /// <param name="toFamily">the font family to replace with</param>
        public void AddFontFamilyMapping(string fromFamily, string toFamily)
        {
            ArgChecker.AssertArgNotNullOrEmpty(fromFamily, "fromFamily");
            ArgChecker.AssertArgNotNullOrEmpty(toFamily, "toFamily");

            this.FontsMapping[fromFamily] = toFamily;
        }

        /// <summary>
        /// Get cached font instance for the given font properties.<br/>
        /// Improve performance not to create same font multiple times.
        /// </summary>
        /// <returns>cached font instance</returns>
        public RFont GetCachedFont(string family, double size, RFontStyle style)
        {
            var font = this.TryGetFont(family, size, style);
            if (font == null)
            {
                if (!this.ExistingFontFamilies.ContainsKey(family))
                {
                    string mappedFamily;
                    if (this.FontsMapping.TryGetValue(family, out mappedFamily))
                    {
                        font = this.TryGetFont(mappedFamily, size, style);
                        if (font == null)
                        {
                            font = this.CreateFont(mappedFamily, size, style);
                            this.FontsCache[mappedFamily][size][style] = font;
                        }
                    }
                }

                if (font == null)
                {
                    font = this.CreateFont(family, size, style);
                }

                this.FontsCache[family][size][style] = font;
            }

            return font;
        }

        #region Private methods

        /// <summary>
        /// Get cached font if it exists in cache or null if it is not.
        /// </summary>
        private RFont TryGetFont(string family, double size, RFontStyle style)
        {
            RFont font = null;
            if (this.FontsCache.ContainsKey(family))
            {
                var a = this.FontsCache[family];
                if (a.ContainsKey(size))
                {
                    var b = a[size];
                    if (b.ContainsKey(style))
                    {
                        font = b[style];
                    }
                }
                else
                {
                    this.FontsCache[family][size] = new Dictionary<RFontStyle, RFont>();
                }
            }
            else
            {
                this.FontsCache[family] = new Dictionary<double, Dictionary<RFontStyle, RFont>>();
                this.FontsCache[family][size] = new Dictionary<RFontStyle, RFont>();
            }

            return font;
        }

        /// <summary>
        // create font (try using existing font family to support custom fonts)
        /// </summary>
        private RFont CreateFont(string family, double size, RFontStyle style)
        {
            RFontFamily fontFamily;
            try
            {
                return this.ExistingFontFamilies.TryGetValue(family, out fontFamily)
                    ? this.Adapter.CreateFont(fontFamily, size, style)
                    : this.Adapter.CreateFont(family, size, style);
            }
            catch
            {
                // handle possibility of no requested style exists for the font, use regular then
                return this.ExistingFontFamilies.TryGetValue(family, out fontFamily)
                    ? this.Adapter.CreateFont(fontFamily, size, RFontStyle.Regular)
                    : this.Adapter.CreateFont(family, size, RFontStyle.Regular);
            }
        }

        #endregion
    }
}
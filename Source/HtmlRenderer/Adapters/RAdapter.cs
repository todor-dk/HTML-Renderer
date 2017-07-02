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
using System.IO;
using Scientia.HtmlRenderer.Adapters.Entities;
using Scientia.HtmlRenderer.Core;
using Scientia.HtmlRenderer.Core.Entities;
using Scientia.HtmlRenderer.Core.Handlers;
using Scientia.HtmlRenderer.Core.Utils;

namespace Scientia.HtmlRenderer.Adapters
{
    /// <summary>
    /// Platform adapter to bridge platform specific objects to HTML Renderer core library.<br/>
    /// Core uses abstract renderer objects (RAdapter/RControl/REtc...) to access platform specific functionality, the concrete platforms
    /// implements those objects to provide concrete platform implementation. Those allowing the core library to be platform agnostic.
    /// <para>
    /// Platforms: WinForms, WPF, Metro, PDF renders, etc.<br/>
    /// Objects: UI elements(Controls), Graphics(Render context), Colors, Brushes, Pens, Fonts, Images, Clipboard, etc.<br/>
    /// </para>
    /// </summary>
    /// <remarks>
    /// It is best to have a singleton instance of this class for concrete implementation!<br/>
    /// This is because it holds caches of default CssData, Images, Fonts and Brushes.
    /// </remarks>
    public abstract class RAdapter
    {
        #region Fields/Consts

        /// <summary>
        /// cache of brush color to brush instance
        /// </summary>
        private readonly Dictionary<RColor, RBrush> BrushesCache = new Dictionary<RColor, RBrush>();

        /// <summary>
        /// cache of pen color to pen instance
        /// </summary>
        private readonly Dictionary<RColor, RPen> PenCache = new Dictionary<RColor, RPen>();

        /// <summary>
        /// cache of all the font used not to create same font again and again
        /// </summary>
        private readonly FontsHandler FontsHandler;

        /// <summary>
        /// default CSS parsed data singleton
        /// </summary>
        private CssData _DefaultCssData;

        /// <summary>
        /// image used to draw loading image icon
        /// </summary>
        private RImage LoadImage;

        /// <summary>
        /// image used to draw error image icon
        /// </summary>
        private RImage ErrorImage;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        protected RAdapter()
        {
            this.FontsHandler = new FontsHandler(this);
        }

        /// <summary>
        /// Get the default CSS stylesheet data.
        /// </summary>
        public CssData DefaultCssData
        {
            get { return this._DefaultCssData ?? (this._DefaultCssData = CssData.Parse(this, CssDefaults.DefaultStyleSheet, false)); }
        }

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        public RColor GetColor(string colorName)
        {
            ArgChecker.AssertArgNotNullOrEmpty(colorName, "colorName");
            return this.GetColorInt(colorName);
        }

        /// <summary>
        /// Get cached pen instance for the given color.
        /// </summary>
        /// <param name="color">the color to get pen for</param>
        /// <returns>pen instance</returns>
        public RPen GetPen(RColor color)
        {
            RPen pen;
            if (!this.PenCache.TryGetValue(color, out pen))
            {
                this.PenCache[color] = pen = this.CreatePen(color);
            }

            return pen;
        }

        /// <summary>
        /// Get cached solid brush instance for the given color.
        /// </summary>
        /// <param name="color">the color to get brush for</param>
        /// <returns>brush instance</returns>
        public RBrush GetSolidBrush(RColor color)
        {
            RBrush brush;
            if (!this.BrushesCache.TryGetValue(color, out brush))
            {
                this.BrushesCache[color] = brush = this.CreateSolidBrush(color);
            }

            return brush;
        }

        /// <summary>
        /// Get linear gradient color brush from <paramref name="color1"/> to <paramref name="color2"/>.
        /// </summary>
        /// <param name="rect">the rectangle to get the brush for</param>
        /// <param name="color1">the start color of the gradient</param>
        /// <param name="color2">the end color of the gradient</param>
        /// <param name="angle">the angle to move the gradient from start color to end color in the rectangle</param>
        /// <returns>linear gradient color brush instance</returns>
        public RBrush GetLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            return this.CreateLinearGradientBrush(rect, color1, color2, angle);
        }

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="RImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        public RImage ConvertImage(object image)
        {
            // TODO:a remove this by creating better API.
            return this.ConvertImageInt(image);
        }

        /// <summary>
        /// Create an <see cref="RImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        public RImage ImageFromStream(Stream memoryStream)
        {
            return this.ImageFromStreamInt(memoryStream);
        }

        /// <summary>
        /// Check if the given font exists in the system by font family name.
        /// </summary>
        /// <param name="font">the font name to check</param>
        /// <returns>true - font exists by given family name, false - otherwise</returns>
        public bool IsFontExists(string font)
        {
            return this.FontsHandler.IsFontExists(font);
        }

        /// <summary>
        /// Adds a font family to be used.
        /// </summary>
        /// <param name="fontFamily">The font family to add.</param>
        public void AddFontFamily(RFontFamily fontFamily)
        {
            this.FontsHandler.AddFontFamily(fontFamily);
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
            this.FontsHandler.AddFontFamilyMapping(fromFamily, toFamily);
        }

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        public RFont GetFont(string family, double size, RFontStyle style)
        {
            return this.FontsHandler.GetCachedFont(family, size, style);
        }

        /// <summary>
        /// Get image to be used while HTML image is loading.
        /// </summary>
        public RImage GetLoadingImage()
        {
            if (this.LoadImage == null)
            {
                var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("TheArtOfDev.HtmlRenderer.Core.Utils.ImageLoad.png");
                if (stream != null)
                    this.LoadImage = this.ImageFromStream(stream);
            }

            return this.LoadImage;
        }

        /// <summary>
        /// Get image to be used if HTML image load failed.
        /// </summary>
        public RImage GetLoadingFailedImage()
        {
            if (this.ErrorImage == null)
            {
                var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("TheArtOfDev.HtmlRenderer.Core.Utils.ImageError.png");
                if (stream != null)
                    this.ErrorImage = this.ImageFromStream(stream);
            }

            return this.ErrorImage;
        }

        /// <summary>
        /// Get data object for the given html and plain text data.<br />
        /// The data object can be used for clipboard or drag-drop operation.<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        /// <returns>drag-drop data object</returns>
        public object GetClipboardDataObject(string html, string plainText)
        {
            return this.GetClipboardDataObjectInt(html, plainText);
        }

        /// <summary>
        /// Set the given text to the clipboard<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="text">the text to set</param>
        public void SetToClipboard(string text)
        {
            this.SetToClipboardInt(text);
        }

        /// <summary>
        /// Set the given html and plain text data to clipboard.<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        public void SetToClipboard(string html, string plainText)
        {
            this.SetToClipboardInt(html, plainText);
        }

        /// <summary>
        /// Set the given image to clipboard.<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="image">the image object to set to clipboard</param>
        public void SetToClipboard(RImage image)
        {
            this.SetToClipboardInt(image);
        }

        /// <summary>
        /// Create a context menu that can be used on the control<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <returns>new context menu</returns>
        public RContextMenu GetContextMenu()
        {
            return this.CreateContextMenuInt();
        }

        /// <summary>
        /// Save the given image to file by showing save dialog to the client.<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="image">the image to save</param>
        /// <param name="name">the name of the image for save dialog</param>
        /// <param name="extension">the extension of the image for save dialog</param>
        /// <param name="control">optional: the control to show the dialog on</param>
        public void SaveToFile(RImage image, string name, string extension, RControl control = null)
        {
            this.SaveToFileInt(image, name, extension, control);
        }

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        internal RFont CreateFont(string family, double size, RFontStyle style)
        {
            return this.CreateFontInt(family, size, style);
        }

        /// <summary>
        /// Get font instance by given font family instance, size and style.<br/>
        /// Used to support custom fonts that require explicit font family instance to be created.
        /// </summary>
        /// <param name="family">the font family instance</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        internal RFont CreateFont(RFontFamily family, double size, RFontStyle style)
        {
            return this.CreateFontInt(family, size, style);
        }

        #region Private/Protected methods

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        protected abstract RColor GetColorInt(string colorName);

        /// <summary>
        /// Get cached pen instance for the given color.
        /// </summary>
        /// <param name="color">the color to get pen for</param>
        /// <returns>pen instance</returns>
        protected abstract RPen CreatePen(RColor color);

        /// <summary>
        /// Get cached solid brush instance for the given color.
        /// </summary>
        /// <param name="color">the color to get brush for</param>
        /// <returns>brush instance</returns>
        protected abstract RBrush CreateSolidBrush(RColor color);

        /// <summary>
        /// Get linear gradient color brush from <paramref name="color1"/> to <paramref name="color2"/>.
        /// </summary>
        /// <param name="rect">the rectangle to get the brush for</param>
        /// <param name="color1">the start color of the gradient</param>
        /// <param name="color2">the end color of the gradient</param>
        /// <param name="angle">the angle to move the gradient from start color to end color in the rectangle</param>
        /// <returns>linear gradient color brush instance</returns>
        protected abstract RBrush CreateLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle);

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="RImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        protected abstract RImage ConvertImageInt(object image);

        /// <summary>
        /// Create an <see cref="RImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        protected abstract RImage ImageFromStreamInt(Stream memoryStream);

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected abstract RFont CreateFontInt(string family, double size, RFontStyle style);

        /// <summary>
        /// Get font instance by given font family instance, size and style.<br/>
        /// Used to support custom fonts that require explicit font family instance to be created.
        /// </summary>
        /// <param name="family">the font family instance</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected abstract RFont CreateFontInt(RFontFamily family, double size, RFontStyle style);

        /// <summary>
        /// Get data object for the given html and plain text data.<br />
        /// The data object can be used for clipboard or drag-drop operation.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        /// <returns>drag-drop data object</returns>
        protected virtual object GetClipboardDataObjectInt(string html, string plainText)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the given text to the clipboard
        /// </summary>
        /// <param name="text">the text to set</param>
        protected virtual void SetToClipboardInt(string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the given html and plain text data to clipboard.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        protected virtual void SetToClipboardInt(string html, string plainText)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the given image to clipboard.
        /// </summary>
        /// <param name="image"></param>
        protected virtual void SetToClipboardInt(RImage image)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a context menu that can be used on the control
        /// </summary>
        /// <returns>new context menu</returns>
        protected virtual RContextMenu CreateContextMenuInt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save the given image to file by showing save dialog to the client.
        /// </summary>
        /// <param name="image">the image to save</param>
        /// <param name="name">the name of the image for save dialog</param>
        /// <param name="extension">the extension of the image for save dialog</param>
        /// <param name="control">optional: the control to show the dialog on</param>
        protected virtual void SaveToFileInt(RImage image, string name, string extension, RControl control = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
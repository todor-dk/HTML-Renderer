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

using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.PdfSharp.Adapters;
using TheArtOfDev.HtmlRenderer.PdfSharp.Utilities;

namespace TheArtOfDev.HtmlRenderer.PdfSharp
{
    /// <summary>
    /// Low level handling of Html Renderer logic, this class is used by <see cref="PdfGenerator"/>.
    /// </summary>
    /// <seealso cref="HtmlContainerInt"/>
    public sealed class HtmlContainer : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// The internal core html container
        /// </summary>
        private readonly HtmlContainerInt _HtmlContainerInt;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public HtmlContainer()
        {
            this._HtmlContainerInt = new HtmlContainerInt(PdfSharpAdapter.Instance);
            this._HtmlContainerInt.AvoidAsyncImagesLoading = true;
            this._HtmlContainerInt.AvoidImagesLateLoading = true;
        }

        /// <summary>
        /// Raised when the set html document has been fully loaded.<br/>
        /// Allows manipulation of the html dom, scroll position, etc.
        /// </summary>
        public event EventHandler LoadComplete
        {
            add { this._HtmlContainerInt.LoadComplete += value; }
            remove { this._HtmlContainerInt.LoadComplete -= value; }
        }

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        /// <remarks>
        /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
        /// </remarks>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError
        {
            add { this._HtmlContainerInt.RenderError += value; }
            remove { this._HtmlContainerInt.RenderError -= value; }
        }

        /// <summary>
        /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or Uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad
        {
            add { this._HtmlContainerInt.StylesheetLoad += value; }
            remove { this._HtmlContainerInt.StylesheetLoad -= value; }
        }

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad
        {
            add { this._HtmlContainerInt.ImageLoad += value; }
            remove { this._HtmlContainerInt.ImageLoad -= value; }
        }

        /// <summary>
        /// The internal core html container
        /// </summary>
        internal HtmlContainerInt HtmlContainerInt
        {
            get { return this._HtmlContainerInt; }
        }

        /// <summary>
        /// the parsed stylesheet data used for handling the html
        /// </summary>
        public CssData CssData
        {
            get { return this._HtmlContainerInt.CssData; }
        }

        /// <summary>
        /// Gets or sets a value indicating if anti-aliasing should be avoided for geometry like backgrounds and borders (default - false).
        /// </summary>
        public bool AvoidGeometryAntialias
        {
            get { return this._HtmlContainerInt.AvoidGeometryAntialias; }
            set { this._HtmlContainerInt.AvoidGeometryAntialias = value; }
        }

        /// <summary>
        /// The scroll offset of the html.<br/>
        /// This will adjust the rendered html by the given offset so the content will be "scrolled".<br/>
        /// </summary>
        /// <example>
        /// Element that is rendered at location (50,100) with offset of (0,200) will not be rendered as it
        /// will be at -100 therefore outside the client rectangle.
        /// </example>
        public XPoint ScrollOffset
        {
            get { return Utils.Convert(this._HtmlContainerInt.ScrollOffset); }
            set { this._HtmlContainerInt.ScrollOffset = Utils.Convert(value); }
        }

        /// <summary>
        /// The top-left most location of the rendered html.<br/>
        /// This will offset the top-left corner of the rendered html.
        /// </summary>
        public XPoint Location
        {
            get { return Utils.Convert(this._HtmlContainerInt.Location); }
            set { this._HtmlContainerInt.Location = Utils.Convert(value); }
        }

        /// <summary>
        /// The max width and height of the rendered html.<br/>
        /// The max width will effect the html layout wrapping lines, resize images and tables where possible.<br/>
        /// The max height does NOT effect layout, but will not render outside it (clip).<br/>
        /// <see cref="ActualSize"/> can be exceed the max size by layout restrictions (unwrappable line, set image size, etc.).<br/>
        /// Set zero for unlimited (width\height separately).<br/>
        /// </summary>
        public XSize MaxSize
        {
            get { return Utils.Convert(this._HtmlContainerInt.MaxSize); }
            set { this._HtmlContainerInt.MaxSize = Utils.Convert(value); }
        }

        /// <summary>
        /// The actual size of the rendered html (after layout)
        /// </summary>
        public XSize ActualSize
        {
            get { return Utils.Convert(this._HtmlContainerInt.ActualSize); }
            internal set { this._HtmlContainerInt.ActualSize = Utils.Convert(value); }
        }

        public XSize PageSize
        {
            get
            {
                return new XSize(this._HtmlContainerInt.PageSize.Width, this._HtmlContainerInt.PageSize.Height);
            }

            set
            {
                this._HtmlContainerInt.PageSize = new RSize(value.Width, value.Height);
            }
        }

        /// <summary>
        /// the top margin between the page start and the text
        /// </summary>
        public int MarginTop
        {
            get
            {
                return this._HtmlContainerInt.MarginTop;
            }

            set
            {
                if (value > -1)
                {
                    this._HtmlContainerInt.MarginTop = value;
                }
            }
        }

        /// <summary>
        /// the bottom margin between the page end and the text
        /// </summary>
        public int MarginBottom
        {
            get
            {
                return this._HtmlContainerInt.MarginBottom;
            }

            set
            {
                if (value > -1)
                {
                    this._HtmlContainerInt.MarginBottom = value;
                }
            }
        }

        /// <summary>
        /// the left margin between the page start and the text
        /// </summary>
        public int MarginLeft
        {
            get
            {
                return this._HtmlContainerInt.MarginLeft;
            }

            set
            {
                if (value > -1)
                {
                    this._HtmlContainerInt.MarginLeft = value;
                }
            }
        }

        /// <summary>
        /// the right margin between the page end and the text
        /// </summary>
        public int MarginRight
        {
            get
            {
                return this._HtmlContainerInt.MarginRight;
            }

            set
            {
                if (value > -1)
                {
                    this._HtmlContainerInt.MarginRight = value;
                }
            }
        }

        /// <summary>
        /// Set all 4 margins to the given value.
        /// </summary>
        /// <param name="value"></param>
        public void SetMargins(int value)
        {
            if (value > -1)
            {
                this._HtmlContainerInt.SetMargins(value);
            }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        public string SelectedText
        {
            get { return this._HtmlContainerInt.SelectedText; }
        }

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        public string SelectedHtml
        {
            get { return this._HtmlContainerInt.SelectedHtml; }
        }

        /// <summary>
        /// Init with optional document and stylesheet.
        /// </summary>
        /// <param name="htmlSource">the html to init with, init empty if not given</param>
        /// <param name="baseCssData">optional: the stylesheet to init with, init default if not given</param>
        public void SetHtml(string htmlSource, CssData baseCssData = null)
        {
            this._HtmlContainerInt.SetHtml(htmlSource, baseCssData);
        }

        /// <summary>
        /// Get html from the current DOM tree with style if requested.
        /// </summary>
        /// <param name="styleGen">Optional: controls the way styles are generated when html is generated (default: <see cref="HtmlGenerationStyle.Inline"/>)</param>
        /// <returns>generated html</returns>
        public string GetHtml(HtmlGenerationStyle styleGen = HtmlGenerationStyle.Inline)
        {
            return this._HtmlContainerInt.GetHtml(styleGen);
        }

        /// <summary>
        /// Get attribute value of element at the given x,y location by given key.<br/>
        /// If more than one element exist with the attribute at the location the inner most is returned.
        /// </summary>
        /// <param name="location">the location to find the attribute at</param>
        /// <param name="attribute">the attribute key to get value by</param>
        /// <returns>found attribute value or null if not found</returns>
        public string GetAttributeAt(XPoint location, string attribute)
        {
            return this._HtmlContainerInt.GetAttributeAt(Utils.Convert(location), attribute);
        }

        /// <summary>
        /// Get all the links in the HTML with the element rectangle and href data.
        /// </summary>
        /// <returns>collection of all the links in the HTML</returns>
        public List<LinkElementData<XRect>> GetLinks()
        {
            var linkElements = new List<LinkElementData<XRect>>();
            foreach (var link in this.HtmlContainerInt.GetLinks())
            {
                linkElements.Add(new LinkElementData<XRect>(link.Id, link.Href, Utils.Convert(link.Rectangle)));
            }

            return linkElements;
        }

        /// <summary>
        /// Get css link href at the given x,y location.
        /// </summary>
        /// <param name="location">the location to find the link at</param>
        /// <returns>css link href if exists or null</returns>
        public string GetLinkAt(XPoint location)
        {
            return this._HtmlContainerInt.GetLinkAt(Utils.Convert(location));
        }

        /// <summary>
        /// Get the rectangle of html element as calculated by html layout.<br/>
        /// Element if found by id (id attribute on the html element).<br/>
        /// Note: to get the screen rectangle you need to adjust by the hosting control.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to get its rectangle</param>
        /// <returns>the rectangle of the element or null if not found</returns>
        public XRect? GetElementRectangle(string elementId)
        {
            var r = this._HtmlContainerInt.GetElementRectangle(elementId);
            return r.HasValue ? Utils.Convert(r.Value) : (XRect?)null;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.
        /// </summary>
        /// <param name="g">Device context to draw</param>
        public void PerformLayout(XGraphics g)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            using (var ig = new GraphicsAdapter(g))
            {
                this._HtmlContainerInt.PerformLayout(ig);
            }
        }

        /// <summary>
        /// Render the html using the given device.
        /// </summary>
        /// <param name="g">the device to use to render</param>
        public void PerformPaint(XGraphics g)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            using (var ig = new GraphicsAdapter(g))
            {
                this._HtmlContainerInt.PerformPaint(ig);
            }
        }

        public void Dispose()
        {
            this._HtmlContainerInt.Dispose();
        }
    }
}
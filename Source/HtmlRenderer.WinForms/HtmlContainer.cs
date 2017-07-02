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
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Scientia.HtmlRenderer.Adapters.Entities;
using Scientia.HtmlRenderer.Core;
using Scientia.HtmlRenderer.Core.Entities;
using Scientia.HtmlRenderer.Core.Parse;
using Scientia.HtmlRenderer.Core.Utils;
using Scientia.HtmlRenderer.WinForms.Adapters;
using Scientia.HtmlRenderer.WinForms.Utilities;

namespace Scientia.HtmlRenderer.WinForms
{
    /// <summary>
    /// Low level handling of Html Renderer logic, this class is used by <see cref="HtmlParser"/>,
    /// <see cref="HtmlLabel"/>, <see cref="HtmlToolTip"/> and <see cref="HtmlRender"/>.<br/>
    /// </summary>
    /// <seealso cref="HtmlContainerInt"/>
    public sealed class HtmlContainer : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// The internal core html container
        /// </summary>
        private readonly HtmlContainerInt _HtmlContainerInt;

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.
        /// </summary>
        private bool _UseGdiPlusTextRendering;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public HtmlContainer()
        {
            this._HtmlContainerInt = new HtmlContainerInt(WinFormsAdapter.Instance);
            this._HtmlContainerInt.SetMargins(0);
            this._HtmlContainerInt.PageSize = new RSize(99999, 99999);
        }

        /// <summary>
        /// Raised when the set html document has been fully loaded.<br/>
        /// Allows manipulation of the html dom, scroll position, etc.
        /// </summary>
        public event EventHandler LoadComplete
        {
            add { this.HtmlContainerInt.LoadComplete += value; }
            remove { this.HtmlContainerInt.LoadComplete -= value; }
        }

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked
        {
            add { this._HtmlContainerInt.LinkClicked += value; }
            remove { this._HtmlContainerInt.LinkClicked -= value; }
        }

        /// <summary>
        /// Raised when html renderer requires refresh of the control hosting (invalidation and re-layout).
        /// </summary>
        /// <remarks>
        /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
        /// </remarks>
        public event EventHandler<HtmlRefreshEventArgs> Refresh
        {
            add { this._HtmlContainerInt.Refresh += value; }
            remove { this._HtmlContainerInt.Refresh -= value; }
        }

        /// <summary>
        /// Raised when Html Renderer request scroll to specific location.<br/>
        /// This can occur on document anchor click.
        /// </summary>
        public event EventHandler<HtmlScrollEventArgs> ScrollChange
        {
            add { this._HtmlContainerInt.ScrollChange += value; }
            remove { this._HtmlContainerInt.ScrollChange -= value; }
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
        /// Use GDI+ text rendering to measure/draw text.<br/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// GDI+ text rendering is less smooth than GDI text rendering but it natively supports alpha channel
        /// thus allows creating transparent images.
        /// </para>
        /// <para>
        /// While using GDI+ text rendering you can control the text rendering using <see cref="Graphics.TextRenderingHint"/>, note that
        /// using <see cref="TextRenderingHint.ClearTypeGridFit"/> doesn't work well with transparent background.
        /// </para>
        /// </remarks>
        public bool UseGdiPlusTextRendering
        {
            get
            {
                return this._UseGdiPlusTextRendering;
            }

            set
            {
                if (this._UseGdiPlusTextRendering != value)
                {
                    this._UseGdiPlusTextRendering = value;
                    this._HtmlContainerInt.RequestRefresh(true);
                }
            }
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
        /// Gets or sets a value indicating if image asynchronous loading should be avoided (default - false).<br/>
        /// True - images are loaded synchronously during html parsing.<br/>
        /// False - images are loaded asynchronously to html parsing when downloaded from URL or loaded from disk.<br/>
        /// </summary>
        /// <remarks>
        /// Asynchronously image loading allows to unblock html rendering while image is downloaded or loaded from disk using IO
        /// ports to achieve better performance.<br/>
        /// Asynchronously image loading should be avoided when the full html content must be available during render, like render to image.
        /// </remarks>
        public bool AvoidAsyncImagesLoading
        {
            get { return this._HtmlContainerInt.AvoidAsyncImagesLoading; }
            set { this._HtmlContainerInt.AvoidAsyncImagesLoading = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if image loading only when visible should be avoided (default - false).<br/>
        /// True - images are loaded as soon as the html is parsed.<br/>
        /// False - images that are not visible because of scroll location are not loaded until they are scrolled to.
        /// </summary>
        /// <remarks>
        /// Images late loading improve performance if the page contains image outside the visible scroll area, especially if there is large
        /// amount of images, as all image loading is delayed (downloading and loading into memory).<br/>
        /// Late image loading may effect the layout and actual size as image without set size will not have actual size until they are loaded
        /// resulting in layout change during user scroll.<br/>
        /// Early image loading may also effect the layout if image without known size above the current scroll location are loaded as they
        /// will push the html elements down.
        /// </remarks>
        public bool AvoidImagesLateLoading
        {
            get { return this._HtmlContainerInt.AvoidImagesLateLoading; }
            set { this._HtmlContainerInt.AvoidImagesLateLoading = value; }
        }

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        public bool IsSelectionEnabled
        {
            get { return this._HtmlContainerInt.IsSelectionEnabled; }
            set { this._HtmlContainerInt.IsSelectionEnabled = value; }
        }

        /// <summary>
        /// Is the build-in context menu enabled and will be shown on mouse right click (default - true)
        /// </summary>
        public bool IsContextMenuEnabled
        {
            get { return this._HtmlContainerInt.IsContextMenuEnabled; }
            set { this._HtmlContainerInt.IsContextMenuEnabled = value; }
        }

        /// <summary>
        /// The scroll offset of the html.<br/>
        /// This will adjust the rendered html by the given offset so the content will be "scrolled".<br/>
        /// </summary>
        /// <example>
        /// Element that is rendered at location (50,100) with offset of (0,200) will not be rendered as it
        /// will be at -100 therefore outside the client rectangle.
        /// </example>
        public Point ScrollOffset
        {
            get { return Utils.ConvertRound(this._HtmlContainerInt.ScrollOffset); }
            set { this._HtmlContainerInt.ScrollOffset = Utils.Convert(value); }
        }

        /// <summary>
        /// The top-left most location of the rendered html.<br/>
        /// This will offset the top-left corner of the rendered html.
        /// </summary>
        public PointF Location
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
        public SizeF MaxSize
        {
            get { return Utils.Convert(this._HtmlContainerInt.MaxSize); }
            set { this._HtmlContainerInt.MaxSize = Utils.Convert(value); }
        }

        /// <summary>
        /// The actual size of the rendered html (after layout)
        /// </summary>
        public SizeF ActualSize
        {
            get { return Utils.Convert(this._HtmlContainerInt.ActualSize); }
            internal set { this._HtmlContainerInt.ActualSize = Utils.Convert(value); }
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
        /// Clear the current selection.
        /// </summary>
        public void ClearSelection()
        {
            this.HtmlContainerInt.ClearSelection();
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
        public string GetAttributeAt(Point location, string attribute)
        {
            return this._HtmlContainerInt.GetAttributeAt(Utils.Convert(location), attribute);
        }

        /// <summary>
        /// Get all the links in the HTML with the element rectangle and href data.
        /// </summary>
        /// <returns>collection of all the links in the HTML</returns>
        public List<LinkElementData<RectangleF>> GetLinks()
        {
            var linkElements = new List<LinkElementData<RectangleF>>();
            foreach (var link in this.HtmlContainerInt.GetLinks())
            {
                linkElements.Add(new LinkElementData<RectangleF>(link.Id, link.Href, Utils.Convert(link.Rectangle)));
            }

            return linkElements;
        }

        /// <summary>
        /// Get css link href at the given x,y location.
        /// </summary>
        /// <param name="location">the location to find the link at</param>
        /// <returns>css link href if exists or null</returns>
        public string GetLinkAt(Point location)
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
        public RectangleF? GetElementRectangle(string elementId)
        {
            var r = this._HtmlContainerInt.GetElementRectangle(elementId);
            return r.HasValue ? Utils.Convert(r.Value) : (RectangleF?)null;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.
        /// </summary>
        /// <param name="g">Device context to draw</param>
        public void PerformLayout(Graphics g)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            using (var ig = new GraphicsAdapter(g, this._UseGdiPlusTextRendering))
            {
                this._HtmlContainerInt.PerformLayout(ig);
            }
        }

        /// <summary>
        /// Render the html using the given device.
        /// </summary>
        /// <param name="g">the device to use to render</param>
        public void PerformPaint(Graphics g)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            using (var ig = new GraphicsAdapter(g, this._UseGdiPlusTextRendering))
            {
                this._HtmlContainerInt.PerformPaint(ig);
            }
        }

        /// <summary>
        /// Handle mouse down to handle selection.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="e">the mouse event args</param>
        public void HandleMouseDown(Control parent, MouseEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            this._HtmlContainerInt.HandleMouseDown(new ControlAdapter(parent, this._UseGdiPlusTextRendering), Utils.Convert(e.Location));
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="e">the mouse event args</param>
        public void HandleMouseUp(Control parent, MouseEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            this._HtmlContainerInt.HandleMouseUp(new ControlAdapter(parent, this._UseGdiPlusTextRendering), Utils.Convert(e.Location), CreateMouseEvent(e));
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        /// <param name="e">mouse event args</param>
        public void HandleMouseDoubleClick(Control parent, MouseEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            this._HtmlContainerInt.HandleMouseDoubleClick(new ControlAdapter(parent, this._UseGdiPlusTextRendering), Utils.Convert(e.Location));
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        /// <param name="e">the mouse event args</param>
        public void HandleMouseMove(Control parent, MouseEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            this._HtmlContainerInt.HandleMouseMove(new ControlAdapter(parent, this._UseGdiPlusTextRendering), Utils.Convert(e.Location));
        }

        /// <summary>
        /// Handle mouse leave to handle hover cursor.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        public void HandleMouseLeave(Control parent)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            this._HtmlContainerInt.HandleMouseLeave(new ControlAdapter(parent, this._UseGdiPlusTextRendering));
        }

        /// <summary>
        /// Handle key down event for selection and copy.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="e">the pressed key</param>
        public void HandleKeyDown(Control parent, KeyEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            this._HtmlContainerInt.HandleKeyDown(new ControlAdapter(parent, this._UseGdiPlusTextRendering), CreateKeyEevent(e));
        }

        public void Dispose()
        {
            this._HtmlContainerInt.Dispose();
        }

        #region Private methods

        /// <summary>
        /// Create HtmlRenderer mouse event from win forms mouse event.
        /// </summary>
        private static RMouseEvent CreateMouseEvent(MouseEventArgs e)
        {
            return new RMouseEvent((e.Button & MouseButtons.Left) != 0);
        }

        /// <summary>
        /// Create HtmlRenderer key event from win forms key event.
        /// </summary>
        private static RKeyEvent CreateKeyEevent(KeyEventArgs e)
        {
            return new RKeyEvent(e.Control, e.KeyCode == Keys.A, e.KeyCode == Keys.C);
        }

        #endregion
    }
}
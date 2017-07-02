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
using Scientia.HtmlRenderer.Adapters;
using Scientia.HtmlRenderer.Adapters.Entities;
using Scientia.HtmlRenderer.Core.Handlers;
using Scientia.HtmlRenderer.Core.Utils;

namespace Scientia.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// CSS box for image element.
    /// </summary>
    internal sealed class CssBoxImage : CssBox
    {
        #region Fields and Consts

        /// <summary>
        /// the image word of this image box
        /// </summary>
        private readonly CssRectImage ImageWord;

        /// <summary>
        /// handler used for image loading by source
        /// </summary>
        private ImageLoadHandler ImageLoadHandler;

        /// <summary>
        /// is image load is finished, used to know if no image is found
        /// </summary>
        private bool ImageLoadingComplete;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="parent">the parent box of this box</param>
        /// <param name="tag">the html tag data of this box</param>
        public CssBoxImage(CssBox parent, HtmlTag tag)
            : base(parent, tag)
        {
            this.ImageWord = new CssRectImage(this);
            this.Words.Add(this.ImageWord);
        }

        /// <summary>
        /// Get the image of this image box.
        /// </summary>
        public RImage Image
        {
            get { return this.ImageWord.Image; }
        }

        /// <summary>
        /// Paints the fragment
        /// </summary>
        /// <param name="g">the device to draw to</param>
        protected override void PaintImp(RGraphics g)
        {
            // load image if it is in visible rectangle
            if (this.ImageLoadHandler == null)
            {
                this.ImageLoadHandler = new ImageLoadHandler(this.HtmlContainer, this.OnLoadImageComplete);
                this.ImageLoadHandler.LoadImage(this.GetAttribute("src"), this.HtmlTag != null ? this.HtmlTag.Attributes : null);
            }

            var rect = CommonUtils.GetFirstValueOrDefault(this.Rectangles);
            RPoint offset = RPoint.Empty;

            if (!this.IsFixed)
                offset = this.HtmlContainer.ScrollOffset;

            rect.Offset(offset);

            var clipped = RenderUtils.ClipGraphicsByOverflow(g, this);

            this.PaintBackground(g, rect, true, true);
            BordersDrawHandler.DrawBoxBorders(g, this, rect, true, true);

            RRect r = this.ImageWord.Rectangle;
            r.Offset(offset);
            r.Height -= this.ActualBorderTopWidth + this.ActualBorderBottomWidth + this.ActualPaddingTop + this.ActualPaddingBottom;
            r.Y += this.ActualBorderTopWidth + this.ActualPaddingTop;
            r.X = Math.Floor(r.X);
            r.Y = Math.Floor(r.Y);

            if (this.ImageWord.Image != null)
            {
                if (r.Width > 0 && r.Height > 0)
                {
                    if (this.ImageWord.ImageRectangle == RRect.Empty)
                        g.DrawImage(this.ImageWord.Image, r);
                    else
                        g.DrawImage(this.ImageWord.Image, r, this.ImageWord.ImageRectangle);

                    if (this.ImageWord.Selected)
                    {
                        g.DrawRectangle(this.GetSelectionBackBrush(g, true), this.ImageWord.Left + offset.X, this.ImageWord.Top + offset.Y, this.ImageWord.Width + 2, DomUtils.GetCssLineBoxByWord(this.ImageWord).LineHeight);
                    }
                }
            }
            else if (this.ImageLoadingComplete)
            {
                if (this.ImageLoadingComplete && r.Width > 19 && r.Height > 19)
                {
                    RenderUtils.DrawImageErrorIcon(g, this.HtmlContainer, r);
                }
            }
            else
            {
                RenderUtils.DrawImageLoadingIcon(g, this.HtmlContainer, r);
                if (r.Width > 19 && r.Height > 19)
                {
                    g.DrawRectangle(g.GetPen(RColor.LightGray), r.X, r.Y, r.Width, r.Height);
                }
            }

            if (clipped)
                g.PopClip();
        }

        /// <summary>
        /// Assigns words its width and height
        /// </summary>
        /// <param name="g">the device to use</param>
        internal override void MeasureWordsSize(RGraphics g)
        {
            if (!this.WordsSizeMeasured)
            {
                if (this.ImageLoadHandler == null && (this.HtmlContainer.AvoidAsyncImagesLoading || this.HtmlContainer.AvoidImagesLateLoading))
                {
                    this.ImageLoadHandler = new ImageLoadHandler(this.HtmlContainer, this.OnLoadImageComplete);

                    if (this.Content != null && this.Content != CssConstants.Normal)
                        this.ImageLoadHandler.LoadImage(this.Content, this.HtmlTag != null ? this.HtmlTag.Attributes : null);
                    else
                        this.ImageLoadHandler.LoadImage(this.GetAttribute("src"), this.HtmlTag != null ? this.HtmlTag.Attributes : null);
                }

                this.MeasureWordSpacing(g);
                this.WordsSizeMeasured = true;
            }

            CssLayoutEngine.MeasureImageSize(this.ImageWord);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (this.ImageLoadHandler != null)
                this.ImageLoadHandler.Dispose();
            base.Dispose();
        }

        #region Private methods

        /// <summary>
        /// Set error image border on the image box.
        /// </summary>
        private void SetErrorBorder()
        {
            this.SetAllBorders(CssConstants.Solid, "2px", "#A0A0A0");
            this.BorderRightColor = this.BorderBottomColor = "#E3E3E3";
        }

        /// <summary>
        /// On image load process is complete with image or without update the image box.
        /// </summary>
        /// <param name="image">the image loaded or null if failed</param>
        /// <param name="rectangle">the source rectangle to draw in the image (empty - draw everything)</param>
        /// <param name="async">is the callback was called async to load image call</param>
        private void OnLoadImageComplete(RImage image, RRect rectangle, bool async)
        {
            this.ImageWord.Image = image;
            this.ImageWord.ImageRectangle = rectangle;
            this.ImageLoadingComplete = true;
            this.WordsSizeMeasured = false;

            if (this.ImageLoadingComplete && image == null)
            {
                this.SetErrorBorder();
            }

            if (!this.HtmlContainer.AvoidImagesLateLoading || async)
            {
                var width = new CssLength(this.Width);
                var height = new CssLength(this.Height);
                var layout = (width.Number <= 0 || width.Unit != CssUnit.Pixels) || (height.Number <= 0 || height.Unit != CssUnit.Pixels);
                this.HtmlContainer.RequestRefresh(layout);
            }
        }

        #endregion
    }
}
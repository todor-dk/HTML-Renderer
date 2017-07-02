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

namespace Scientia.HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Because PdfSharp doesn't support texture brush we need to implement it ourselves.
    /// </summary>
    internal sealed class XTextureBrush
    {
        #region Fields/Consts

        /// <summary>
        /// The image to draw in the brush
        /// </summary>
        private readonly XImage Image;

        /// <summary>
        /// the
        /// </summary>
        private readonly XRect DstRect;

        /// <summary>
        /// the transform the location of the image to handle center alignment
        /// </summary>
        private readonly XPoint TranslateTransformLocation;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public XTextureBrush(XImage image, XRect dstRect, XPoint translateTransformLocation)
        {
            this.Image = image;
            this.DstRect = dstRect;
            this.TranslateTransformLocation = translateTransformLocation;
        }

        /// <summary>
        /// Draw the texture image in the given graphics at the given location.
        /// </summary>
        public void DrawRectangle(XGraphics g, double x, double y, double width, double height)
        {
            var prevState = g.Save();
            g.IntersectClip(new XRect(x, y, width, height));

            double rx = this.TranslateTransformLocation.X;
            double w = this.Image.PixelWidth, h = this.Image.PixelHeight;
            while (rx < x + width)
            {
                double ry = this.TranslateTransformLocation.Y;
                while (ry < y + height)
                {
                    g.DrawImage(this.Image, rx, ry, w, h);
                    ry += h;
                }

                rx += w;
            }

            g.Restore(prevState);
        }
    }
}
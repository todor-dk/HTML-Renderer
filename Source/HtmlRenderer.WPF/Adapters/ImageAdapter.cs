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

using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF Image object for core.
    /// </summary>
    internal sealed class ImageAdapter : RImage
    {
        /// <summary>
        /// the underline WPF image.
        /// </summary>
        private readonly BitmapImage _Image;

        /// <summary>
        /// Init.
        /// </summary>
        public ImageAdapter(BitmapImage image)
        {
            this._Image = image;
        }

        /// <summary>
        /// the underline WPF image.
        /// </summary>
        public BitmapImage Image
        {
            get { return this._Image; }
        }

        public override double Width
        {
            get { return this._Image.PixelWidth; }
        }

        public override double Height
        {
            get { return this._Image.PixelHeight; }
        }

        public override void Dispose()
        {
            if (this._Image.StreamSource != null)
            {
                this._Image.StreamSource.Dispose();
            }
        }
    }
}
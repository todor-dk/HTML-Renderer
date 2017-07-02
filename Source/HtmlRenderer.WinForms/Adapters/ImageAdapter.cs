﻿// "Therefore those skilled at the unorthodox
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

using System.Drawing;
using Scientia.HtmlRenderer.Adapters;

namespace Scientia.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Image object for core.
    /// </summary>
    internal sealed class ImageAdapter : RImage
    {
        /// <summary>
        /// the underline win-forms image.
        /// </summary>
        private readonly Image _Image;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ImageAdapter(Image image)
        {
            this._Image = image;
        }

        /// <summary>
        /// the underline win-forms image.
        /// </summary>
        public Image Image
        {
            get { return this._Image; }
        }

        public override double Width
        {
            get { return this._Image.Width; }
        }

        public override double Height
        {
            get { return this._Image.Height; }
        }

        public override void Dispose()
        {
            this._Image.Dispose();
        }
    }
}
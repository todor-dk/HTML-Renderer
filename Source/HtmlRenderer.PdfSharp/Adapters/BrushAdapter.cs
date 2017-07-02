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

using System;
using Scientia.HtmlRenderer.Adapters;
using PdfSharp.Drawing;

namespace Scientia.HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for WinForms brushes objects for core.
    /// </summary>
    internal sealed class BrushAdapter : RBrush
    {
        /// <summary>
        /// The actual PdfSharp brush instance.<br/>
        /// Should be <see cref="XBrush"/> but there is some fucking issue inheriting from it =/
        /// </summary>
        private readonly Object _Brush;

        /// <summary>
        /// Init.
        /// </summary>
        public BrushAdapter(Object brush)
        {
            this._Brush = brush;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public Object Brush
        {
            get { return this._Brush; }
        }

        public override void Dispose()
        {
        }
    }
}
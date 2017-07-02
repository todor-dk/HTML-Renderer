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

using System.Drawing;
using Scientia.HtmlRenderer.Adapters;

namespace Scientia.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms brushes objects for core.
    /// </summary>
    internal sealed class BrushAdapter : RBrush
    {
        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        private readonly Brush _Brush;

        /// <summary>
        /// If to dispose the brush when <see cref="Dispose"/> is called.<br/>
        /// Ignore dispose for cached brushes.
        /// </summary>
        private readonly bool DisposeBrush;

        /// <summary>
        /// Init.
        /// </summary>
        public BrushAdapter(Brush brush, bool dispose)
        {
            this._Brush = brush;
            this.DisposeBrush = dispose;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public Brush Brush
        {
            get { return this._Brush; }
        }

        public override void Dispose()
        {
            if (this.DisposeBrush)
            {
                this._Brush.Dispose();
            }
        }
    }
}
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
using Scientia.HtmlRenderer.Adapters.Entities;
using Scientia.HtmlRenderer.Adapters;

namespace Scientia.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms pens objects for core.
    /// </summary>
    internal sealed class PenAdapter : RPen
    {
        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        private readonly Pen _Pen;

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(Pen pen)
        {
            this._Pen = pen;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public Pen Pen
        {
            get { return this._Pen; }
        }

        public override double Width
        {
            get { return this._Pen.Width; }
            set { this._Pen.Width = (float)value; }
        }

        public override RDashStyle DashStyle
        {
            set
            {
                switch (value)
                {
                    case RDashStyle.Solid:
                        this._Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        break;
                    case RDashStyle.Dash:
                        this._Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        if (this.Width < 2)
                        {
                            this._Pen.DashPattern = new[] { 4, 4f }; // better looking
                        }

                        break;
                    case RDashStyle.Dot:
                        this._Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        break;
                    case RDashStyle.DashDot:
                        this._Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                        break;
                    case RDashStyle.DashDotDot:
                        this._Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
                        break;
                    case RDashStyle.Custom:
                        this._Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                        break;
                    default:
                        this._Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        break;
                }
            }
        }
    }
}
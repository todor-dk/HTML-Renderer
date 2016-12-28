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

using System.Windows.Media;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF pens objects for core.
    /// </summary>
    internal sealed class PenAdapter : RPen
    {
        /// <summary>
        /// The actual WPF brush instance.
        /// </summary>
        private readonly Brush Brush;

        /// <summary>
        /// the width of the pen
        /// </summary>
        private double _Width;

        /// <summary>
        /// the dash style of the pen
        /// </summary>
        private DashStyle _DashStyle = DashStyles.Solid;

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(Brush brush)
        {
            this.Brush = brush;
        }

        public override double Width
        {
            get { return this._Width; }
            set { this._Width = value; }
        }

        public override RDashStyle DashStyle
        {
            set
            {
                switch (value)
                {
                    case RDashStyle.Solid:
                        this._DashStyle = DashStyles.Solid;
                        break;
                    case RDashStyle.Dash:
                        this._DashStyle = DashStyles.Dash;
                        break;
                    case RDashStyle.Dot:
                        this._DashStyle = DashStyles.Dot;
                        break;
                    case RDashStyle.DashDot:
                        this._DashStyle = DashStyles.DashDot;
                        break;
                    case RDashStyle.DashDotDot:
                        this._DashStyle = DashStyles.DashDotDot;
                        break;
                    default:
                        this._DashStyle = DashStyles.Solid;
                        break;
                }
            }
        }

        /// <summary>
        /// Create the actual WPF pen instance.
        /// </summary>
        public Pen CreatePen()
        {
            var pen = new Pen(this.Brush, this._Width);
            pen.DashStyle = this._DashStyle;
            return pen;
        }
    }
}
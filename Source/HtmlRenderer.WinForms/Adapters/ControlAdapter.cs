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

using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.WinForms.Utilities;

namespace TheArtOfDev.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Control for core.
    /// </summary>
    internal sealed class ControlAdapter : RControl
    {
        /// <summary>
        /// the underline win forms control.
        /// </summary>
        private readonly Control _Control;

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.
        /// </summary>
        private readonly bool UseGdiPlusTextRendering;

        /// <summary>
        /// Init.
        /// </summary>
        public ControlAdapter(Control control, bool useGdiPlusTextRendering)
            : base(WinFormsAdapter.Instance)
        {
            ArgChecker.AssertArgNotNull(control, "control");

            this._Control = control;
            this.UseGdiPlusTextRendering = useGdiPlusTextRendering;
        }

        /// <summary>
        /// Get the underline win forms control
        /// </summary>
        public Control Control
        {
            get { return this._Control; }
        }

        public override RPoint MouseLocation
        {
            get { return Utils.Convert(this._Control.PointToClient(Control.MousePosition)); }
        }

        public override bool LeftMouseButton
        {
            get { return (Control.MouseButtons & MouseButtons.Left) != 0; }
        }

        public override bool RightMouseButton
        {
            get { return (Control.MouseButtons & MouseButtons.Right) != 0; }
        }

        public override void SetCursorDefault()
        {
            this._Control.Cursor = Cursors.Default;
        }

        public override void SetCursorHand()
        {
            this._Control.Cursor = Cursors.Hand;
        }

        public override void SetCursorIBeam()
        {
            this._Control.Cursor = Cursors.IBeam;
        }

        public override void DoDragDropCopy(object dragDropData)
        {
            this._Control.DoDragDrop(dragDropData, DragDropEffects.Copy);
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            using (var g = new GraphicsAdapter(this._Control.CreateGraphics(), this.UseGdiPlusTextRendering, true))
            {
                g.MeasureString(str, font, maxWidth, out charFit, out charFitWidth);
            }
        }

        public override void Invalidate()
        {
            this._Control.Invalidate();
        }
    }
}
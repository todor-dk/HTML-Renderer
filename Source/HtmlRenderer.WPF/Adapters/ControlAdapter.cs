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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scientia.HtmlRenderer.Adapters;
using Scientia.HtmlRenderer.Adapters.Entities;
using Scientia.HtmlRenderer.Core.Utils;
using Scientia.HtmlRenderer.WPF.Utilities;

namespace Scientia.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF Control for core.
    /// </summary>
    internal sealed class ControlAdapter : RControl
    {
        /// <summary>
        /// the underline WPF control.
        /// </summary>
        private readonly Control _Control;

        /// <summary>
        /// Init.
        /// </summary>
        public ControlAdapter(Control control)
            : base(WpfAdapter.Instance)
        {
            ArgChecker.AssertArgNotNull(control, "control");

            this._Control = control;
        }

        /// <summary>
        /// Get the underline WPF control
        /// </summary>
        public Control Control
        {
            get { return this._Control; }
        }

        public override RPoint MouseLocation
        {
            get { return Utils.Convert(this._Control.PointFromScreen(Mouse.GetPosition(this._Control))); }
        }

        public override bool LeftMouseButton
        {
            get { return Mouse.LeftButton == MouseButtonState.Pressed; }
        }

        public override bool RightMouseButton
        {
            get { return Mouse.RightButton == MouseButtonState.Pressed; }
        }

        public override void SetCursorDefault()
        {
            this._Control.Cursor = Cursors.Arrow;
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
            DragDrop.DoDragDrop(this._Control, dragDropData, DragDropEffects.Copy);
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            using (var g = new GraphicsAdapter())
            {
                g.MeasureString(str, font, maxWidth, out charFit, out charFitWidth);
            }
        }

        public override void Invalidate()
        {
            this._Control.InvalidateVisual();
        }
    }
}
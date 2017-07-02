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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Scientia.HtmlRenderer.Demo.Common;

namespace Scientia.HtmlRenderer.Demo.WinForms
{
    public partial class SampleForm : Form
    {
        private readonly Bitmap Background;

        public SampleForm()
        {
            this.InitializeComponent();

            this.Icon = DemoForm.GetIcon();

            this._htmlLabel.Text = DemoUtils.SampleHtmlLabelText;
            this._htmlPanel.Text = DemoUtils.SampleHtmlPanelText;

            this.Background = HtmlRenderingHelper.CreateImageForTransparentBackground();
        }

        private void OnHtmlLabelClick(object sender, EventArgs e)
        {
            this._pGrid.SelectedObject = this._htmlLabel;
        }

        private void OnHtmlPanelClick(object sender, EventArgs e)
        {
            this._pGrid.SelectedObject = this._htmlPanel;
        }

        private void OnHtmlLabelHostingPanelPaint(object sender, PaintEventArgs e)
        {
            using (var b = new TextureBrush(this.Background, WrapMode.Tile))
            {
                e.Graphics.FillRectangle(b, this._htmlLabelHostingPanel.ClientRectangle);
            }
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            this._htmlToolTip.SetToolTip(this._changeTooltipButton, this._htmlLabel.Text);
        }

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ((Control)this._pGrid.SelectedObject).Refresh();
            this.Refresh();
        }
    }
}
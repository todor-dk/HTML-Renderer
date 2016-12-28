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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.Demo.WinForms
{
    public partial class GenerateImageForm : Form
    {
        private readonly string Html;
        private readonly Bitmap Background;

        public GenerateImageForm(string html)
        {
            this.Html = html;
            this.InitializeComponent();

            this.Icon = DemoForm.GetIcon();

            this.Background = HtmlRenderingHelper.CreateImageForTransparentBackground();

            foreach (var color in GetColors())
            {
                if (color != Color.Transparent)
                {
                    this._backgroundColorTSB.Items.Add(color.Name);
                }
            }

            this._backgroundColorTSB.SelectedItem = Color.White.Name;

            foreach (var hint in Enum.GetNames(typeof(TextRenderingHint)))
            {
                this._textRenderingHintTSCB.Items.Add(hint);
            }

            this._textRenderingHintTSCB.SelectedItem = TextRenderingHint.AntiAlias.ToString();

            this._useGdiPlusTSB.Enabled = !HtmlRenderingHelper.IsRunningOnMono();
            this._backgroundColorTSB.Enabled = !HtmlRenderingHelper.IsRunningOnMono();
        }

        private void OnSaveToFile_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Images|*.png;*.bmp;*.jpg";
                saveDialog.FileName = "image";
                saveDialog.DefaultExt = ".png";

                var dialogResult = saveDialog.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    this._pictureBox.Image.Save(saveDialog.FileName);
                }
            }
        }

        private void OnUseGdiPlus_Click(object sender, EventArgs e)
        {
            this._useGdiPlusTSB.Checked = !this._useGdiPlusTSB.Checked;
            this._textRenderingHintTSCB.Visible = this._useGdiPlusTSB.Checked;
            this._backgroundColorTSB.Visible = !this._useGdiPlusTSB.Checked;
            this._toolStripLabel.Text = this._useGdiPlusTSB.Checked ? "Text Rendering Hint:" : "Background:";
            this.GenerateImage();
        }

        private void OnBackgroundColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.GenerateImage();
        }

        private void TextRenderingHintTSCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.GenerateImage();
        }

        private void OnGenerateImage_Click(object sender, EventArgs e)
        {
            this.GenerateImage();
        }

        private void GenerateImage()
        {
            if (this._backgroundColorTSB.SelectedItem != null && this._textRenderingHintTSCB.SelectedItem != null)
            {
                var backgroundColor = Color.FromName(this._backgroundColorTSB.SelectedItem.ToString());
                TextRenderingHint textRenderingHint = (TextRenderingHint)Enum.Parse(typeof(TextRenderingHint), this._textRenderingHintTSCB.SelectedItem.ToString());

                Image img;
                if (this._useGdiPlusTSB.Checked || HtmlRenderingHelper.IsRunningOnMono())
                {
                    img = HtmlRender.RenderToImageGdiPlus(this.Html, this._pictureBox.ClientSize, textRenderingHint, null, DemoUtils.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoad);
                }
                else
                {
                    EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = DemoUtils.OnStylesheetLoad;
                    EventHandler<HtmlImageLoadEventArgs> imageLoad = HtmlRenderingHelper.OnImageLoad;
                    var objects = new object[] { this.Html, this._pictureBox.ClientSize, backgroundColor, null, stylesheetLoad, imageLoad };

                    var types = new[] { typeof(String), typeof(Size), typeof(Color), typeof(CssData), typeof(EventHandler<HtmlStylesheetLoadEventArgs>), typeof(EventHandler<HtmlImageLoadEventArgs>) };
                    var m = typeof(HtmlRender).GetMethod("RenderToImage", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, types, null);
                    img = (Image)m.Invoke(null, objects);
                }

                this._pictureBox.Image = img;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            using (var b = new TextureBrush(this.Background, WrapMode.Tile))
            {
                e.Graphics.FillRectangle(b, this.ClientRectangle);
            }
        }

        private static List<Color> GetColors()
        {
            const MethodAttributes attributes = MethodAttributes.Static | MethodAttributes.Public;
            PropertyInfo[] properties = typeof(Color).GetProperties();
            List<Color> list = new List<Color>();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo info = properties[i];
                if (info.PropertyType == typeof(Color))
                {
                    MethodInfo getMethod = info.GetGetMethod();
                    if ((getMethod != null) && ((getMethod.Attributes & attributes) == attributes))
                    {
                        list.Add((Color)info.GetValue(null, null));
                    }
                }
            }

            return list;
        }
    }
}
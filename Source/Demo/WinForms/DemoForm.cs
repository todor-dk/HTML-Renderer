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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using Scientia.HtmlRenderer.Demo.Common;
using Scientia.HtmlRenderer.PdfSharp;
using Scientia.HtmlRenderer.WinForms;
using PdfSharp;

namespace Scientia.HtmlRenderer.Demo.WinForms
{
    public partial class DemoForm : Form
    {
        #region Fields/Consts

        /// <summary>
        /// the private font used for the demo
        /// </summary>
        private readonly PrivateFontCollection PrivateFont = new PrivateFontCollection();

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public DemoForm()
        {
            SamplesLoader.Init(HtmlRenderingHelper.IsRunningOnMono() ? "Mono" : "WinForms", typeof(HtmlRender).Assembly.GetName().Version.ToString());

            this.InitializeComponent();

            this.Icon = GetIcon();
            this._openSampleFormTSB.Image = Common.Properties.Resources.form;
            this._showIEViewTSSB.Image = Common.Properties.Resources.browser;
            this._openInExternalViewTSB.Image = Common.Properties.Resources.chrome;
            this._useGeneratedHtmlTSB.Image = Common.Properties.Resources.code;
            this._generateImageSTB.Image = Common.Properties.Resources.image;
            this._generatePdfTSB.Image = Common.Properties.Resources.pdf;
            this._runPerformanceTSB.Image = Common.Properties.Resources.stopwatch;

            this.StartPosition = FormStartPosition.CenterScreen;
            var size = Screen.GetWorkingArea(Point.Empty);
            this.Size = new Size((int)(size.Width * 0.7), (int)(size.Height * 0.8));

            this.LoadCustomFonts();

            this._showIEViewTSSB.Enabled = !HtmlRenderingHelper.IsRunningOnMono();
            this._generatePdfTSB.Enabled = !HtmlRenderingHelper.IsRunningOnMono();
        }

        /// <summary>
        /// Load custom fonts to be used by renderer HTMLs
        /// </summary>
        private void LoadCustomFonts()
        {
            // load custom font font into private fonts collection
            var file = Path.GetTempFileName();
            File.WriteAllBytes(file, Resources.CustomFont);
            this.PrivateFont.AddFontFile(file);

            // add the fonts to renderer
            foreach (var fontFamily in this.PrivateFont.Families)
            {
                HtmlRender.AddFontFamily(fontFamily);
            }
        }

        /// <summary>
        /// Get icon for the demo.
        /// </summary>
        internal static Icon GetIcon()
        {
            var stream = typeof(DemoForm).Assembly.GetManifestResourceStream("TheArtOfDev.HtmlRenderer.Demo.WinForms.html.ico");
            return stream != null ? new Icon(stream) : null;
        }

        private void OnOpenSampleForm_Click(object sender, EventArgs e)
        {
            using (var f = new SampleForm())
            {
                f.ShowDialog();
            }
        }

        /// <summary>
        /// Toggle if to show split view of HtmlPanel and WinForms WebBrowser control.
        /// </summary>
        private void OnShowIEView_ButtonClick(object sender, EventArgs e)
        {
            this._showIEViewTSSB.Checked = !this._showIEViewTSSB.Checked;
            this._mainControl.ShowWebBrowserView(this._showIEViewTSSB.Checked);
        }

        /// <summary>
        /// Open the current html is external process - the default user browser.
        /// </summary>
        private void OnOpenInExternalView_Click(object sender, EventArgs e)
        {
            var tmpFile = Path.ChangeExtension(Path.GetTempFileName(), ".htm");
            File.WriteAllText(tmpFile, this._mainControl.GetHtml());
            Process.Start(tmpFile);
        }

        /// <summary>
        /// Toggle the use generated html button state.
        /// </summary>
        private void OnUseGeneratedHtml_Click(object sender, EventArgs e)
        {
            this._useGeneratedHtmlTSB.Checked = !this._useGeneratedHtmlTSB.Checked;
            this._mainControl.UseGeneratedHtml = this._useGeneratedHtmlTSB.Checked;
            this._mainControl.UpdateWebBrowserHtml();
        }

        /// <summary>
        /// Open generate image form for the current html.
        /// </summary>
        private void OnGenerateImage_Click(object sender, EventArgs e)
        {
            using (var f = new GenerateImageForm(this._mainControl.GetHtml()))
            {
                f.ShowDialog();
            }
        }

        /// <summary>
        /// Create PDF using PdfSharp project, save to file and open that file.
        /// </summary>
        private void OnGeneratePdf_Click(object sender, EventArgs e)
        {
            PdfGenerateConfig config = new PdfGenerateConfig();
            config.PageSize = PageSize.A4;
            config.SetMargins(20);

            var doc = PdfGenerator.GeneratePdf(this._mainControl.GetHtml(), config, null, DemoUtils.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoadPdfSharp);
            var tmpFile = Path.GetTempFileName();
            tmpFile = Path.GetFileNameWithoutExtension(tmpFile) + ".pdf";
            doc.Save(tmpFile);
            Process.Start(tmpFile);
        }

        /// <summary>
        /// Execute performance test by setting all sample HTMLs in a loop.
        /// </summary>
        private void OnRunPerformance_Click(object sender, EventArgs e)
        {
            this._mainControl.UpdateLock = true;
            this._toolStrip.Enabled = false;
            Application.DoEvents();

            var msg = DemoUtils.RunSamplesPerformanceTest(html =>
            {
                this._mainControl.SetHtml(html);
                Application.DoEvents(); // so paint will be called
            });

            Clipboard.SetDataObject(msg);
            MessageBox.Show(msg, "Test run results");

            this._mainControl.UpdateLock = false;
            this._toolStrip.Enabled = true;
        }
    }
}
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
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Scientia.HtmlRenderer.Core.Entities;
using Scientia.HtmlRenderer.Demo.Common;
using Timer = System.Threading.Timer;

namespace Scientia.HtmlRenderer.Demo.WinForms
{
    public partial class MainControl : UserControl
    {
        #region Fields and Consts

        /// <summary>
        /// the name of the tree node root for all performance samples
        /// </summary>
        private const string PerformanceSamplesTreeNodeName = "Performance Samples";

        /// <summary>
        /// timer to update the rendered html when html in editor changes with delay
        /// </summary>
        private readonly Timer UpdateHtmlTimer;

        /// <summary>
        /// used ignore html editor updates when updating separately
        /// </summary>
        private bool _UpdateLock;

        /// <summary>
        /// In IE view if to show original html or the html generated from the html control
        /// </summary>
        private bool _UseGeneratedHtml;

        #endregion

        public MainControl()
        {
            this.InitializeComponent();

            this._htmlPanel.RenderError += OnRenderError;
            this._htmlPanel.LinkClicked += OnLinkClicked;
            this._htmlPanel.StylesheetLoad += DemoUtils.OnStylesheetLoad;
            this._htmlPanel.ImageLoad += HtmlRenderingHelper.OnImageLoad;
            this._htmlToolTip.ImageLoad += HtmlRenderingHelper.OnImageLoad;
            this._htmlPanel.LoadComplete += (sender, args) => this._htmlPanel.ScrollToElement("C4");

            this._htmlToolTip.SetToolTip(this._htmlPanel, Resources.Tooltip);

            this._htmlEditor.Font = new Font(FontFamily.GenericMonospace, 10);

            this.LoadSamples();

            this.UpdateHtmlTimer = new Timer(this.OnUpdateHtmlTimerTick);
        }

        /// <summary>
        /// used ignore html editor updates when updating separately
        /// </summary>
        public bool UpdateLock
        {
            get { return this._UpdateLock; }
            set { this._UpdateLock = value; }
        }

        /// <summary>
        /// In IE view if to show original html or the html generated from the html control
        /// </summary>
        public bool UseGeneratedHtml
        {
            get { return this._UseGeneratedHtml; }
            set { this._UseGeneratedHtml = value; }
        }

        /// <summary>
        /// Show\Hide the web browser viewer.
        /// </summary>
        public void ShowWebBrowserView(bool show)
        {
            this._webBrowser.Visible = show;
            this._splitter.Visible = show;

            if (this._webBrowser.Visible)
            {
                this._webBrowser.Width = this._splitContainer2.Panel2.Width / 2;
                this.UpdateWebBrowserHtml();
            }
        }

        /// <summary>
        /// Update the html shown in the web browser
        /// </summary>
        public void UpdateWebBrowserHtml()
        {
            if (this._webBrowser.Visible)
            {
                this._webBrowser.DocumentText = this._UseGeneratedHtml ? this._htmlPanel.GetHtml() : this.GetFixedHtml();
            }
        }

        public string GetHtml()
        {
            return this._UseGeneratedHtml ? this._htmlPanel.GetHtml() : this._htmlEditor.Text;
        }

        public void SetHtml(string html)
        {
            this._htmlPanel.Text = html;
        }

        #region Private methods

        /// <summary>
        /// Loads the tree of document samples
        /// </summary>
        private void LoadSamples()
        {
            var showcaseRoot = new TreeNode("HTML Renderer");
            this._samplesTreeView.Nodes.Add(showcaseRoot);

            foreach (var sample in SamplesLoader.ShowcaseSamples)
            {
                this.AddTreeNode(showcaseRoot, sample);
            }

            var testSamplesRoot = new TreeNode("Test Samples");
            this._samplesTreeView.Nodes.Add(testSamplesRoot);

            foreach (var sample in SamplesLoader.TestSamples)
            {
                this.AddTreeNode(testSamplesRoot, sample);
            }

            if (SamplesLoader.PerformanceSamples.Count > 0)
            {
                var perfTestSamplesRoot = new TreeNode(PerformanceSamplesTreeNodeName);
                this._samplesTreeView.Nodes.Add(perfTestSamplesRoot);

                foreach (var sample in SamplesLoader.PerformanceSamples)
                {
                    this.AddTreeNode(perfTestSamplesRoot, sample);
                }
            }

            showcaseRoot.Expand();

            if (showcaseRoot.Nodes.Count > 0)
            {
                this._samplesTreeView.SelectedNode = showcaseRoot.Nodes[0];
            }
        }

        /// <summary>
        /// Add an html sample to the tree and to all samples collection
        /// </summary>
        private void AddTreeNode(TreeNode root, HtmlSample sample)
        {
            var node = new TreeNode(sample.Name);
            node.Tag = new HtmlSample(sample.Name, sample.FullName, sample.Html);
            root.Nodes.Add(node);
        }

        /// <summary>
        /// On tree view node click load the html to the html panel and html editor.
        /// </summary>
        private void OnSamplesTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            var sample = e.Node.Tag as HtmlSample;
            if (sample != null)
            {
                this._UpdateLock = true;

                if (!HtmlRenderingHelper.IsRunningOnMono() && e.Node.Parent.Text != PerformanceSamplesTreeNodeName)
                {
                    this.SetColoredText(sample.Html);
                }
                else
                {
                    this._htmlEditor.Text = sample.Html;
                }

                Application.UseWaitCursor = true;

                try
                {
                    this._htmlPanel.AvoidImagesLateLoading = !sample.FullName.Contains("Many images");

                    this._htmlPanel.Text = sample.Html;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Failed to render HTML");
                }

                Application.UseWaitCursor = false;
                this._UpdateLock = false;

                this.UpdateWebBrowserHtml();
            }
        }

        /// <summary>
        /// On text change in the html editor update
        /// </summary>
        private void OnHtmlEditorTextChanged(object sender, EventArgs e)
        {
            if (!this._UpdateLock)
            {
                this.UpdateHtmlTimer.Change(1000, int.MaxValue);
            }
        }

        /// <summary>
        /// Update the html renderer with text from html editor.
        /// </summary>
        private void OnUpdateHtmlTimerTick(object state)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                this._UpdateLock = true;

                try
                {
                    this._htmlPanel.Text = this._htmlEditor.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Failed to render HTML");
                }

                // SyntaxHilight.AddColoredText(_htmlEditor.Text, _htmlEditor);
                this.UpdateWebBrowserHtml();

                this._UpdateLock = false;
            }));
        }

        /// <summary>
        /// Fix the raw html by replacing bridge object properties calls with path to file with the data returned from the property.
        /// </summary>
        /// <returns>fixed html</returns>
        private string GetFixedHtml()
        {
            var html = this._htmlEditor.Text;

            html = Regex.Replace(
                html,
                @"src=\""(\w.*?)\""",
                match =>
                {
                    var img = HtmlRenderingHelper.TryLoadResourceImage(match.Groups[1].Value);
                    if (img != null)
                    {
                        var tmpFile = Path.GetTempFileName();
                        img.Save(tmpFile, ImageFormat.Jpeg);
                        return string.Format("src=\"{0}\"", tmpFile);
                    }
                    return match.Value;
                },
                RegexOptions.IgnoreCase);

            html = Regex.Replace(
                html,
                @"href=\""(\w.*?)\""",
                match =>
                {
                    var stylesheet = DemoUtils.GetStylesheet(match.Groups[1].Value);
                    if (stylesheet != null)
                    {
                        var tmpFile = Path.GetTempFileName();
                        File.WriteAllText(tmpFile, stylesheet);
                        return string.Format("href=\"{0}\"", tmpFile);
                    }
                    return match.Value;
                },
                RegexOptions.IgnoreCase);

            return html;
        }

        /// <summary>
        /// Reload the html shown in the html editor by running coloring again.
        /// </summary>
        private void OnReloadColorsLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.SetColoredText(this._htmlEditor.Text);
        }

        /// <summary>
        /// Show error raised from html renderer.
        /// </summary>
        private static void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
            MessageBox.Show(e.Message + (e.Exception != null ? "\r\n" + e.Exception : null), "Error in Html Renderer", MessageBoxButtons.OK);
        }

        /// <summary>
        /// On specific link click handle it here.
        /// </summary>
        private static void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            if (e.Link == "SayHello")
            {
                MessageBox.Show("Hello you!");
                e.Handled = true;
            }
            else if (e.Link == "ShowSampleForm")
            {
                using (var f = new SampleForm())
                {
                    f.ShowDialog();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Set html syntax color text on the RTF html editor.
        /// </summary>
        private void SetColoredText(string text)
        {
            var selectionStart = this._htmlEditor.SelectionStart;
            this._htmlEditor.Clear();
            this._htmlEditor.Rtf = HtmlSyntaxHighlighter.Process(text);
            this._htmlEditor.SelectionStart = selectionStart;
        }

        #endregion
    }
}
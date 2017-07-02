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

using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Scientia.HtmlRenderer.Demo.Common;
using Scientia.HtmlRenderer.WPF;
using Microsoft.Win32;

namespace Scientia.HtmlRenderer.Demo.WPF
{
    /// <summary>
    /// Interaction logic for GenerateImageWindow.xaml
    /// </summary>
    public partial class GenerateImageWindow
    {
        private readonly string Html;
        private BitmapFrame GeneratedImage;

        public GenerateImageWindow(string html)
        {
            this.Html = html;

            this.InitializeComponent();

            this.Loaded += (sender, args) => this.GenerateImage();
        }

        private void OnSaveToFile_click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Images|*.png;*.bmp;*.jpg;*.tif;*.gif;*.wmp;";
            saveDialog.FileName = "image";
            saveDialog.DefaultExt = ".png";

            var dialogResult = saveDialog.ShowDialog(this);
            if (dialogResult.GetValueOrDefault())
            {
                var encoder = HtmlRenderingHelper.GetBitmapEncoder(Path.GetExtension(saveDialog.FileName));
                encoder.Frames.Add(this.GeneratedImage);
                using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.OpenOrCreate))
                {
                    encoder.Save(stream);
                }
            }
        }

        private void OnGenerateImage_Click(object sender, RoutedEventArgs e)
        {
            this.GenerateImage();
        }

        private void GenerateImage()
        {
            if (this._imageBoxBorder.RenderSize.Width > 0 && this._imageBoxBorder.RenderSize.Height > 0)
            {
                this.GeneratedImage = HtmlRender.RenderToImage(this.Html, this._imageBoxBorder.RenderSize, null, DemoUtils.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoad);
                this._imageBox.Source = this.GeneratedImage;
            }
        }
    }
}
using HtmlRenderer.TestLib.Dom.Persisting;
using Microsoft.Win32;
using Scientia.HtmlRenderer;
using Scientia.HtmlRenderer.Dom;
using Scientia.HtmlRenderer.Html5.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HtmlRenderer.DomParseTester
{
    /// <summary>
    /// Interaction logic for ReferenceDomExplorer.xaml
    /// </summary>
    public partial class ReferenceDomExplorer : Window
    {
        public ReferenceDomExplorer()
        {
            InitializeComponent();
        }
        
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadDom_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (!(dlg.ShowDialog(this) ?? false))
                return;
            if (String.IsNullOrWhiteSpace(dlg.FileName))
                return;

            string txt = File.ReadAllText(dlg.FileName);
            var root = TestLib.Dom.Persisting.TextReader.FromData(txt);

            this.DomTree.DataContext = root;
            this.DomTree.ItemsSource = new object[] { root };
        }

        private void LoadHtml_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (!(dlg.ShowDialog(this) ?? false))
                return;
            if (String.IsNullOrWhiteSpace(dlg.FileName))
                return;

            TestLib.Dom.ReferenceNode root;
            using (FileStream fs = File.OpenRead(dlg.FileName))
            {
                // Parse the HTML
                StreamHtmlStream stream = new StreamHtmlStream(fs);
                BrowsingContext browsingContext = new BrowsingContext();
                Scientia.HtmlRenderer.Dom.Document document = browsingContext.ParseDocument(stream, "url:unknown");
                root = TestLib.Dom.ReferenceDocument.FromDocument(document);
            }

            this.DomTree.DataContext = root;
            this.DomTree.ItemsSource = new object[] { root };
        }
    }
}

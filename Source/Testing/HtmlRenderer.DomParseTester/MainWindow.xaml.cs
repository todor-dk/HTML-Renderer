using HtmlRenderer.DomParseTester.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HtmlRenderer.DomParseTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenKeywordsViewer_Click(object sender, RoutedEventArgs e)
        {
            KeywordsView window = new KeywordsView();
            window.Owner = this;
            window.Show();
        }

        private void OpenDomComparer_Click(object sender, RoutedEventArgs e)
        {
            DomComparing.DomComparer window = new DomComparing.DomComparer();
            window.Owner = this;
            window.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenDomExplorer_Click(object sender, RoutedEventArgs e)
        {
            ReferenceDomExplorer window = new ReferenceDomExplorer();
            window.Owner = this;
            window.Show();
        }
    }
}

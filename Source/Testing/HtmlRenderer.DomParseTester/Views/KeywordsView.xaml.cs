using System;
using System.Collections.Generic;
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

namespace HtmlRenderer.DomParseTester.Views
{
    /// <summary>
    /// Interaction logic for KeywordsView.xaml
    /// </summary>
    public partial class KeywordsView : Window
    {
        public KeywordsView()
        {
            this.DataContext = new ViewModels.KeywordsViewModel();
            this.InitializeComponent();
        }

        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ViewModels.KeywordsViewModel)this.DataContext).SelectedKeywords = this.KeywordsList.SelectedItems.OfType<Model.Keyword>().ToArray();
        }
    }
}

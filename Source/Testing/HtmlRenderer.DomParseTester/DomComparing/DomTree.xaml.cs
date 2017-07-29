using HtmlRenderer.DomParseTester.DomComparing.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HtmlRenderer.DomParseTester.DomComparing
{
    /// <summary>
    /// Interaction logic for DomTree.xaml
    /// </summary>
    public partial class DomTree : TreeView
    {
        public DomTree()
        {
            InitializeComponent();
        }



        public Node DomNode
        {
            get { return (Node)GetValue(DomNodeProperty); }
            set { SetValue(DomNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DomNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DomNodeProperty =
            DependencyProperty.Register("DomNode", typeof(Node), typeof(DomTree), new PropertyMetadata(null, DomTree.DomNodeChanged));

        private static void DomNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DomTree self = (DomTree)d;
            self.DataContext = e.NewValue;
            self.ItemsSource = new object[] { e.NewValue };
        }
    }
}

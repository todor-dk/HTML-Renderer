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



        public Node RootNode
        {
            get { return (Node)GetValue(RootNodeProperty); }
            set { SetValue(RootNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RootNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RootNodeProperty =
            DependencyProperty.Register("RootNode", typeof(Node), typeof(DomTree), new PropertyMetadata(null, DomTree.RootNodeChanged));

        private static void RootNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DomTree self = (DomTree)d;
            self.DataContext = e.NewValue;
            self.ItemsSource = new object[] { e.NewValue };
        }

        public Node SelectedNode
        {
            get { return (Node)this.GetValue(DomTree.SelectedNodeProperty); }
            set { this.SetValue(DomTree.SelectedNodeProperty, value); }
        }

        /// <summary>
        /// Dependency property definition for the <see cref="SelectedNode"/> property.
        /// </summary>
        public static readonly DependencyProperty SelectedNodeProperty =
            DependencyProperty.Register(nameof(DomTree.SelectedNode), typeof(Node), typeof(DomTree),
                new PropertyMetadata(null));

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);
            this.SelectedNode = this.SelectedItem as Node;
        }

    }
}

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
    /// Interaction logic for AttributesList.xaml
    /// </summary>
    public partial class AttributesList : ListView
    {
        public Node DomNode
        {
            get { return (Node)this.GetValue(AttributesList.DomNodeProperty); }
            set { this.SetValue(AttributesList.DomNodeProperty, value); }
        }

        /// <summary>
        /// Dependency property definition for the <see cref="DomNode"/> property.
        /// </summary>
        public static readonly DependencyProperty DomNodeProperty =
            DependencyProperty.Register(nameof(AttributesList.DomNode), typeof(Node), typeof(AttributesList),
                new PropertyMetadata(null, AttributesList.DomNodeChanged));

        /// <summary>
        /// Called when the <see cref="DomNode"/> property changes.
        /// </summary>
        /// <param name="d">The AttributesList on which the property has changed value.</param>
        /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this property.</param>     
        private static void DomNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AttributesList self = (AttributesList)d;
            self.DomNodeChanged((Node)e.OldValue, (Node)e.NewValue);
        }

        /// <summary>
        /// Called when the <see cref="DomNode"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the <see cref="DomNode"/> property before the change.</param>
        /// <param name="newValue">The new value of the <see cref="DomNode"/> property after the change.</param>
        private void DomNodeChanged(Node oldValue, Node newValue)
        {
            this.ItemsSource = (newValue as Element)?.Model?.Attributes;
        }

        public AttributesList()
        {
            InitializeComponent();
        }
    }
}

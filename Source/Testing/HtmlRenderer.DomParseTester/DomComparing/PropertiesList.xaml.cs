using Scientia.HtmlRenderer.Dom;
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
    /// Interaction logic for PropertiesList.xaml
    /// </summary>
    public partial class PropertiesList : UserControl
    {
        public PropertiesList()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }
        
        public ViewModels.Node DomNode
        {
            get { return (ViewModels.Node)this.GetValue(PropertiesList.DomNodeProperty); }
            set { this.SetValue(PropertiesList.DomNodeProperty, value); }
        }

        /// <summary>
        /// Dependency property definition for the <see cref="DomNode"/> property.
        /// </summary>
        public static readonly DependencyProperty DomNodeProperty =
            DependencyProperty.Register(nameof(PropertiesList.DomNode), typeof(ViewModels.Node), typeof(PropertiesList),
                new PropertyMetadata(null, DomNodeChanged));

        private static void DomNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertiesList)d).DomNodeChanged((ViewModels.Node)e.OldValue, (ViewModels.Node)e.NewValue);
        }

        private void DomNodeChanged(ViewModels.Node oldValue, ViewModels.Node newValue)
        {
            this.Properties = PropertyDefinition.GetProperties(newValue?.GetModel(), newValue?.CompareResult ?? TestLib.CompareResult.Equal);
        }

        public PropertyDefinition[] Properties
        {
            get { return (PropertyDefinition[])this.GetValue(PropertiesList.PropertiesProperty); }
            set { this.SetValue(PropertiesList.PropertiesProperty, value); }
        }

        /// <summary>
        /// Dependency property definition for the <see cref="Properties"/> property.
        /// </summary>
        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register(nameof(PropertiesList.Properties), typeof(PropertyDefinition[]), typeof(PropertiesList),
                new PropertyMetadata(null));


        public class PropertyDefinition : DependencyObject
        {
            public PropertyDefinition(Node node, string path, bool isValid)
            {
                this.Name = path;

                this.IsValid = isValid;

                Binding binding = new Binding(path);
                binding.Source = node;
                binding.Converter = Internal.TextConverter.Current;
                BindingOperations.SetBinding(this, ValueProperty, binding);
            }

            public bool IsValid { get; private set; }

            public string Name { get; private set; }

            public object Value
            {
                get { return (object)this.GetValue(PropertyDefinition.ValueProperty); }
                set { this.SetValue(PropertyDefinition.ValueProperty, value); }
            }

            /// <summary>
            /// Dependency property definition for the <see cref="Value"/> property.
            /// </summary>
            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register(nameof(PropertyDefinition.Value), typeof(object), typeof(PropertyDefinition),
                    new PropertyMetadata(null));

            public static PropertyDefinition[] GetProperties(Node node, TestLib.CompareResult cr)
            {
                if (node == null)
                    return Array.Empty<PropertyDefinition>();

                List<PropertyDefinition> props = new List<PropertyDefinition>();

                if (node is Node)
                {
                    props.Add(new PropertyDefinition(node, nameof(Node.BaseUri), (cr & TestLib.CompareResult.Node_BaseUri) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Node.NodeName), (cr & TestLib.CompareResult.Node_NodeName) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Node.NodeType), (cr & TestLib.CompareResult.Node_NodeType) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Node.NodeValue), (cr & TestLib.CompareResult.Node_NodeValue) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Node.TextContent), (cr & TestLib.CompareResult.Node_TextContent) == 0));
                }
                if (node is CharacterData)
                {
                    props.Add(new PropertyDefinition(node, nameof(CharacterData.Data), (cr & TestLib.CompareResult.CharacterData_Data) == 0));
                    props.Add(new PropertyDefinition(node, nameof(CharacterData.Length), (cr & TestLib.CompareResult.CharacterData_Length) == 0));
                }
                if (node is Document)
                {
                    props.Add(new PropertyDefinition(node, nameof(Document.QuirksMode), (cr & TestLib.CompareResult.Document_QuirksMode) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Document.Url), (cr & TestLib.CompareResult.Document_Url) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Document.DocumentUri), (cr & TestLib.CompareResult.Document_DocumentUri) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Document.Origin), (cr & TestLib.CompareResult.Document_Origin) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Document.CompatMode), (cr & TestLib.CompareResult.Document_CompatMode) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Document.CharacterSet), (cr & TestLib.CompareResult.Document_CharacterSet) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Document.ContentType), (cr & TestLib.CompareResult.Document_ContentType) == 0));

                }
                if (node is DocumentType)
                {
                    props.Add(new PropertyDefinition(node, nameof(DocumentType.Name), (cr & TestLib.CompareResult.DocumentType_Name) == 0));
                    props.Add(new PropertyDefinition(node, nameof(DocumentType.PublicId), (cr & TestLib.CompareResult.DocumentType_PublicId) == 0));
                    props.Add(new PropertyDefinition(node, nameof(DocumentType.SystemId), (cr & TestLib.CompareResult.DocumentType_SystemId) == 0));
                }
                if (node is Element)
                {
                    props.Add(new PropertyDefinition(node, nameof(Element.NamespaceUri), (cr & TestLib.CompareResult.Element_NamespaceUri) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Element.Prefix), (cr & TestLib.CompareResult.Element_Prefix) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Element.LocalName), (cr & TestLib.CompareResult.Element_LocalName) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Element.TagName), (cr & TestLib.CompareResult.Element_TagName) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Element.Id), (cr & TestLib.CompareResult.Element_Id) == 0));
                    props.Add(new PropertyDefinition(node, nameof(Element.ClassName), (cr & TestLib.CompareResult.Element_ClassName) == 0));
                }
                if (node is ParentNode)
                {
                    props.Add(new PropertyDefinition(node, nameof(ParentNode.ChildElementCount), (cr & TestLib.CompareResult.ChildCountMismatch) == 0));
                }
                if (node is ProcessingInstruction)
                {
                    props.Add(new PropertyDefinition(node, nameof(ProcessingInstruction.Target), (cr & TestLib.CompareResult.ProcessingInstruction_Target) == 0));
                }
                if (node is Text)
                {
                    props.Add(new PropertyDefinition(node, nameof(Text.WholeText), (cr & TestLib.CompareResult.Text_WholeText) == 0));
                }

                return props.OrderBy(pd => pd.Name).ToArray();
            }
        }
    }
}

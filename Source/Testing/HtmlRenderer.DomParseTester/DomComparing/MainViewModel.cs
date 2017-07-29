using HtmlRenderer.DomParseTester.DomComparing.ViewModels;
using HtmlRenderer.DomParseTester.ViewModels;
using HtmlRenderer.TestLib.Dom;
using Microsoft.Win32;
using Scientia.HtmlRenderer;
using Scientia.HtmlRenderer.Html5.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HtmlRenderer.DomParseTester.DomComparing
{
    public class MainViewModel : DependencyObject
    {


        public Node Left
        {
            get { return (ViewModels.Node)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Left.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(ViewModels.Node), typeof(MainViewModel), new PropertyMetadata(null));



        public Node Right
        {
            get { return (Node)GetValue(RightProperty); }
            set { SetValue(RightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Right.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightProperty =
            DependencyProperty.Register("Right", typeof(Node), typeof(MainViewModel), new PropertyMetadata(null));



        public Context Context
        {
            get { return (Context)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Context.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(Context), typeof(MainViewModel), new PropertyMetadata(null));



        public ICommand LeftOpenDomCommand { get; private set; }

        public ICommand LeftOpenHtmlCommand { get; private set; }

        public ICommand RightOpenDomCommand { get; private set; }

        public ICommand RightOpenHtmlCommand { get; private set; }

        public MainViewModel()
        {
            this.LeftOpenDomCommand = new GenericCommand(this.LeftOpenDom);
            this.LeftOpenHtmlCommand = new GenericCommand(this.LeftOpenHtml);
            this.RightOpenDomCommand = new GenericCommand(this.RightOpenDom);
            this.RightOpenHtmlCommand = new GenericCommand(this.RightOpenHtml);
            this.Context = new Context();
        }

        private void LeftOpenDom()
        {
            this.OpenDom(node => this.Left = node);
        }

        private void LeftOpenHtml()
        {
            this.OpenHtml(node => this.Left = node);
        }

        private void RightOpenDom()
        {
            this.OpenDom(node => this.Right = node);
        }

        private void RightOpenHtml()
        {
            this.OpenHtml(node => this.Right = node);
        }

        private void OpenDom(Action<Node> action)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.DefaultExt = ".dom";
            dlg.Filter = "DOM Files|*.dom|All Files|*.*";
            dlg.FilterIndex = 0;
            dlg.Multiselect = false;
            dlg.ShowReadOnly = true;
            dlg.Title = "Open DOM";
            dlg.ValidateNames = true;
            if (!(dlg.ShowDialog() ?? false))
                return;
            if (String.IsNullOrWhiteSpace(dlg.FileName))
                return;

            string txt = File.ReadAllText(dlg.FileName);
            ReferenceNode root = TestLib.Dom.Persisting.TextReader.FromData(txt);

            Node node = Node.FromReferenceNode(this.Context, root);
            action(node);
            this.Compare(this.Left, this.Right);
        }

        private void OpenHtml(Action<Node> action)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.DefaultExt = ".html";
            dlg.Filter = "HTML Files|*.htm;*.html|All Files|*.*";
            dlg.FilterIndex = 0;
            dlg.Multiselect = false;
            dlg.ShowReadOnly = true;
            dlg.Title = "Open HTML";
            dlg.ValidateNames = true;
            if (!(dlg.ShowDialog() ?? false))
                return;
            if (String.IsNullOrWhiteSpace(dlg.FileName))
                return;

            string html = File.ReadAllText(dlg.FileName);

            // Parse the HTML
            StringHtmlStream stream = new StringHtmlStream(html);
            BrowsingContext browsingContext = new BrowsingContext();
            var document = browsingContext.ParseDocument(stream, "url:unknown");

            ReferenceNode root = ReferenceDocument.FromDocument(document);

            Node node = Node.FromReferenceNode(this.Context, root);
            action(node);
            this.Compare(this.Left, this.Right);
        }

        private CompareResult Compare(Node left, Node right)
        {
            CompareResult result = left.Compare(right);
            result = this.Context.GetCompareResult(result);

            if ((left != null) && (right != null))
            {
                if (left.ChildNodes.Count != right.ChildNodes.Count)
                {
                    result = result | CompareResult.ChildCountMismatch;
                }
                else
                {
                    for (int i = 0; i < left.ChildNodes.Count; i++)
                    {
                        Node leftChild = left.ChildNodes[i];
                        Node rightChild = right.ChildNodes[i];
                        CompareResult childResult = this.Compare(leftChild, rightChild);
                        if (childResult != CompareResult.Equal)
                            result = result | CompareResult.ChildDifferent;
                    }
                }
            }

            if (left != null)
                left.CompareResult = result;
            if (right != null)
                right.CompareResult = result;

            return result;
        }
    }
}

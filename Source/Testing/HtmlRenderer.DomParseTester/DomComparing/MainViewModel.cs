using HtmlRenderer.DomParseTester.DomComparing.ViewModels;
using HtmlRenderer.DomParseTester.ViewModels;
using HtmlRenderer.TestLib;
using HtmlRenderer.TestLib.Dom;
using Microsoft.Win32;
using Scientia.HtmlRenderer;
using Scientia.HtmlRenderer.Html5.Parsing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<RecentItem> RecentItems { get; private set; }

        public MainViewModel()
        {
            this.LeftOpenDomCommand = new GenericCommand(this.LeftOpenDom);
            this.LeftOpenHtmlCommand = new GenericCommand(this.LeftOpenHtml);
            this.RightOpenDomCommand = new GenericCommand(this.RightOpenDom);
            this.RightOpenHtmlCommand = new GenericCommand(this.RightOpenHtml);
            this.RecentItems = new ObservableCollection<RecentItem>();
            this.Context = new Context();
            this.LoadRecent();
        }

        private void LeftOpenDom()
        {
            this.OpenDom(node => this.Left = node);
        }

        private void LeftOpenHtml()
        {
            this.OpenHtml(node => this.Left = node);
        }

        public void LeftOpenRecent(RecentItem recent)
        {
            this.OpenRecent(recent, node => this.Left = node);
        }

        private void RightOpenDom()
        {
            this.OpenDom(node => this.Right = node);
        }

        private void RightOpenHtml()
        {
            this.OpenHtml(node => this.Right = node);
        }


        public void RightOpenRecent(RecentItem recent)
        {
            this.OpenRecent(recent, node => this.Right = node);
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

            this.UpdateRecent(dlg.FileName, RecentItem.FileType.Dom);
            this.CompareAndValidate();
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

            this.UpdateRecent(dlg.FileName, RecentItem.FileType.Html);
            this.CompareAndValidate();
        }

        private void OpenRecent(RecentItem recent, Action<Node> action)
        {
            if (recent == null)
                return;
            string path = recent.Path;
            if (!File.Exists(path))
                return;

            ReferenceNode root;
            if (recent.Type == RecentItem.FileType.Html)
            {
                string html = File.ReadAllText(path);

                // Parse the HTML
                StringHtmlStream stream = new StringHtmlStream(html);
                BrowsingContext browsingContext = new BrowsingContext();
                var document = browsingContext.ParseDocument(stream, "url:unknown");

                root = ReferenceDocument.FromDocument(document);
            }
            else
            {
                string txt = File.ReadAllText(path);
                root = TestLib.Dom.Persisting.TextReader.FromData(txt);
            }

            Node node = Node.FromReferenceNode(this.Context, root);
            action(node);

            this.CompareAndValidate();
        }

        private void UpdateRecent(string path, RecentItem.FileType type)
        {
            RecentItem recent = new RecentItem(path, type);
            foreach (RecentItem item in this.RecentItems.Where(i => i.Equals(recent)).ToArray())
                this.RecentItems.Remove(item);

            this.RecentItems.Insert(0, recent);

            string txt = String.Join(";", this.RecentItems.Select(i => ((i.Type == RecentItem.FileType.Dom) ? "D" : "H") + i.Path));

            Properties.Settings.Default.RecentItems = txt;
            Properties.Settings.Default.Save();
        }

        private void LoadRecent()
        {
            string txt = Properties.Settings.Default.RecentItems ?? String.Empty;
            string[] items = txt.Split(';');
            foreach(string item in items)
            {
                if (item.Length == 0)
                    continue;
                RecentItem.FileType type = (item[0] == 'D') ? RecentItem.FileType.Dom : RecentItem.FileType.Html;
                this.RecentItems.Add(new RecentItem(item.Substring(1), type));
            }
        }

        private void CompareAndValidate()
        {
            CompareContext cc = new CompareContext(this.Left, this.Right);
            cc.IgnoredCompareResult = CompareResult.Node_BaseUri | CompareResult.Document_CharacterSet | CompareResult.Document_DocumentUri | CompareResult.Document_Url | CompareResult.Document_Origin;

            if ((this.Left != null) && (this.Right != null))
                cc.CompareRecursive(this.Left.GetModel(), this.Right.GetModel());
            if (this.Left != null)
                cc.ValidateRecursive(this.Left.GetModel(), null, null, null, null, null, null);
            if (this.Right != null)
                cc.ValidateRecursive(this.Right.GetModel(), null, null, null, null, null, null);
        }
    }
}

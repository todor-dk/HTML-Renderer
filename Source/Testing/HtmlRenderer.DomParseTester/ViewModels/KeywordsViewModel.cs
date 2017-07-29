using HtmlRenderer.DomParseTester.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HtmlRenderer.DomParseTester.ViewModels
{
    public class KeywordsViewModel : DependencyObject
    {
        public GenericCommand OpenCommand { get; private set; }

        public GenericCommand SaveCommand { get; private set; }

        public GenericCommand FindUrlsCommand { get; private set; }

        public GenericCommand DownloadUrlsCommand { get; private set; }

        public KeywordsViewModel()
        {
            this.OpenCommand = new GenericCommand(this.Open_Execute);
            this.SaveCommand = new GenericCommand(this.Save_Execute, this.Save_CanExecute);
            this.FindUrlsCommand = new GenericCommand(this.FindUrls_Execute, this.FindUrls_CanExecute);
            this.DownloadUrlsCommand = new GenericCommand(this.DownloadUrls_Execute, this.DownloadUrls_CanExecute);
        }

        private void Open_Execute()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.AddExtension = true;
            dlg.DefaultExt = ".txt";
            dlg.DereferenceLinks = true;
            dlg.Filter = "Text Files|*.txt|All Files|*.*";
            dlg.FilterIndex = 0;
            dlg.InitialDirectory = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))), "TestData");
            dlg.Multiselect = false;
            dlg.Title = "Open Keywords List";
            dlg.ValidateNames = true;

            if (!(dlg.ShowDialog() ?? false))
                return;

            this.Keywords = new KeywordCollection(dlg.FileName);
        }

        private bool Save_CanExecute()
        {
            return this.Keywords != null;
        }

        private void Save_Execute()
        {
            if (this.Keywords == null)
                return;
            Keyword.SaveKeywords(this.Keywords.Path, this.Keywords.Keywords);
        }

        private bool FindUrls_CanExecute()
        {
            return (this.SelectedKeywords != null) && this.SelectedKeywords.Any();
        }

        private void FindUrls_Execute()
        {
            if (this.SelectedKeywords == null)
                return;
            Keyword[] words = this.SelectedKeywords.ToArray();
            if (words.Length == 0)
                return;

            Task.Run(() => this.FindUrls(words));
        }

        private void FindUrls(Keyword[] keywords)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                Keyword keyword = keywords[i];
                string[] urls = this.FindUrls(keyword);
                this.Dispatcher.InvokeAsync(() => { keyword.Urls.Clear(); keyword.Urls.AddRange(urls); } );
                this.Dispatcher.InvokeAsync(() => this.Progress = (i + 1.0) / keywords.Length); 
            }
            this.Dispatcher.InvokeAsync(() => this.Progress = 0);
        }

        private string[] FindUrls(Keyword keyword)
        {
            HashSet<string> urls = new HashSet<string>(keyword.Urls);
            while (urls.Count < 500)
            {
                try
                {
                    int oldcount = urls.Count;
                    List<SearchResult> results = BingWebSearcher.Search(keyword.Text, 500, urls.Count);
                    urls.AddRange(results.Select(res => res.Link));
                    if (urls.Count == oldcount)
                        break;
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            return urls.OrderBy(e => e).ToArray();
        }

        private bool DownloadUrls_CanExecute()
        {
            return (this.SelectedKeywords != null) && this.SelectedKeywords.Any();
        }

        private void DownloadUrls_Execute()
        {
            if (this.SelectedKeywords == null)
                return;
            Keyword[] words = this.SelectedKeywords.ToArray();
            if (words.Length == 0)
                return;

            string basePath = Path.GetDirectoryName(this.Keywords.Path);
            basePath = Path.Combine(basePath, "Files");

            Tuple<string, string>[] urls = words.SelectMany(word => word.Urls.Select(url => new Tuple<string, string>(word.Text, url))).ToArray();
            Task.Run(() => this.DownloadUrls(urls, basePath));
        }

        private void DownloadUrls(Tuple<string, string>[] urls, string basePath)
        {
            int i = 0;
            Parallel.ForEach(urls, url =>
            {
                this.DownloadUrl(url.Item1, url.Item2, basePath);
                this.Dispatcher.InvokeAsync(() => this.Progress = (i++ + 1.0) / urls.Length);
            });
            this.Dispatcher.InvokeAsync(() => this.Progress = 0);
        }

        private readonly char[] InvalidChars = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Distinct().ToArray();

        private void DownloadUrl(string keyword, string url, string basePath)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(url);
            buffer = MD5.Create().ComputeHash(buffer);
            string hash = String.Join("", buffer.Select(b => String.Format("{0:X2}", b)));

            string safeKeyword = new string(keyword.Select(ch => InvalidChars.Contains(ch) ? '_' : ch).ToArray());
            string path = Path.Combine(basePath, safeKeyword);
            Directory.CreateDirectory(path);

            path = Path.Combine(path, hash);
            if (File.Exists(path + ".html"))
                return;

            try
            {    
                string fullUrl = url;
                if ((url.IndexOf("http://", StringComparison.OrdinalIgnoreCase) != 0) && (url.IndexOf("https://", StringComparison.OrdinalIgnoreCase) != 0))
                    fullUrl = "http://" + fullUrl;
                WebClient client = new MyWebClient();
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                client.DownloadFile(fullUrl, path + ".html");
                File.WriteAllText(path + ".txt", String.Format("Keyword: {0}\r\nUrl: {1}", keyword, url));
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText(path + ".txt", String.Format("Keyword: {0}\r\nUrl: {1}\r\nERROR: {2}", keyword, url, ex.Message));
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2.Message);
                }
            }
        }

        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                if (request != null)
                    request.Timeout = 10000;
                return request;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public KeywordCollection Keywords
        {
            get { return (KeywordCollection)this.GetValue(KeywordsViewModel.KeywordsProperty); }
            set { this.SetValue(KeywordsViewModel.KeywordsProperty, value); }
        }

        /// <summary>
        /// Dependency property definition for the <see cref="Keywords"/> property.
        /// </summary>
        public static readonly DependencyProperty KeywordsProperty =
            DependencyProperty.Register(nameof(KeywordsViewModel.Keywords), typeof(KeywordCollection), typeof(KeywordsViewModel),
                new PropertyMetadata(null, KeywordsViewModel.KeywordsChanged));

        /// <summary>
        /// Called when the <see cref="Keywords"/> property changes.
        /// </summary>
        /// <param name="d">The KeywordsViewModel on which the property has changed value.</param>
        /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this property.</param>     
        private static void KeywordsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KeywordsViewModel self = (KeywordsViewModel)d;
            self.KeywordsChanged((KeywordCollection)e.OldValue, (KeywordCollection)e.NewValue);
        }

        /// <summary>
        /// Called when the <see cref="Keywords"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the <see cref="Keywords"/> property before the change.</param>
        /// <param name="newValue">The new value of the <see cref="Keywords"/> property after the change.</param>
        private void KeywordsChanged(KeywordCollection oldValue, KeywordCollection newValue)
        {
            this.SaveCommand.OnCanExecuteChanged();
            this.FindUrlsCommand.OnCanExecuteChanged();
            this.DownloadUrlsCommand.OnCanExecuteChanged();
        }


        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Keyword> SelectedKeywords
        {
            get { return (IEnumerable<Keyword>)this.GetValue(KeywordsViewModel.SelectedKeywordsProperty); }
            set { this.SetValue(KeywordsViewModel.SelectedKeywordsProperty, value); }
        }

        /// <summary>
        /// Dependency property definition for the <see cref="SelectedKeywords"/> property.
        /// </summary>
        public static readonly DependencyProperty SelectedKeywordsProperty =
            DependencyProperty.Register(nameof(KeywordsViewModel.SelectedKeywords), typeof(IEnumerable<Keyword>), typeof(KeywordsViewModel),
                new PropertyMetadata(null, KeywordsViewModel.SelectedKeywordsChanged));

        /// <summary>
        /// Called when the <see cref="SelectedKeywords"/> property changes.
        /// </summary>
        /// <param name="d">The KeywordsViewModel on which the property has changed value.</param>
        /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this property.</param>     
        private static void SelectedKeywordsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KeywordsViewModel self = (KeywordsViewModel)d;
            self.SelectedKeywordsChanged((IEnumerable<Keyword>)e.OldValue, (IEnumerable<Keyword>)e.NewValue);
        }

        /// <summary>
        /// Called when the <see cref="SelectedKeywords"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the <see cref="SelectedKeywords"/> property before the change.</param>
        /// <param name="newValue">The new value of the <see cref="SelectedKeywords"/> property after the change.</param>
        private void SelectedKeywordsChanged(IEnumerable<Keyword> oldValue, IEnumerable<Keyword> newValue)
        {
            this.FindUrlsCommand.OnCanExecuteChanged();
            this.DownloadUrlsCommand.OnCanExecuteChanged();
        }


        /// <summary>
        /// 
        /// </summary>
        public double Progress
        {
            get { return (double)this.GetValue(KeywordsViewModel.ProgressProperty); }
            set { this.SetValue(KeywordsViewModel.ProgressProperty, value); }
        }

        /// <summary>
        /// Dependency property definition for the <see cref="Progress"/> property.
        /// </summary>
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(KeywordsViewModel.Progress), typeof(double), typeof(KeywordsViewModel),
                new PropertyMetadata(null));


    }
}

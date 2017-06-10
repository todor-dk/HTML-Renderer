using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace HtmlRenderer.ExperimentalApp.Models
{
    public class Keyword
    {
        private string Section;

        public string Text { get; set; }

        public string Urls { get; set; }

        public bool IsResolved
        {
            get { return !String.IsNullOrEmpty(this.Urls); }
        }

        public Keyword(string text, string urls = null)
        {
            this.Text = text;
            this.Urls = urls;
        }

        public static readonly Keyword[] Keywords = Keyword.LoadKeywords(@"C:\DEV_ATL\GitHub\HTML-Renderer\HTML-Renderer\Source\Testing\HtmlRenderer.ExperimentalApp\Data\Keywords.txt");

        public static Keyword[] LoadKeywords(string path)
        {
            using (Stream file = File.OpenRead(path))
            {
                using (StreamReader reader = new StreamReader(file, System.Text.Encoding.UTF8))
                {
                    string section = null;
                    Keyword keyword = null;
                    List<Keyword> result = new List<Keyword>();
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            return result.ToArray();

                        string trimmed = line.Trim();
                        if (trimmed.Length == 0)
                            continue;

                        if (trimmed.StartsWith("//"))
                        {
                            section = line;
                            continue;
                        }

                        if (line[0] == '\t')
                        {
                            if (keyword != null)
                            {
                                if (String.IsNullOrEmpty(keyword.Urls))
                                    keyword.Urls = trimmed;
                                else
                                    keyword.Urls = keyword.Urls + "\n" + trimmed;
                            }

                            continue;
                        }

                        keyword = new Keyword(trimmed);
                        keyword.Section = section;
                        section = null;
                        result.Add(keyword);
                    }
                }
            }
        }

        public static void SaveKeywords(string path, IEnumerable<Keyword> words)
        {
            lock(SyncLock)
            {
                SaveKeywordsWorker(path, words);
            }
        } 

        private static readonly object SyncLock = new object();

        public static void SaveKeywordsWorker(string path, IEnumerable<Keyword> words)
        {
            using (Stream file = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamWriter writer = new StreamWriter(file, System.Text.Encoding.UTF8))
                {
                    foreach (Keyword keyword in words)
                    {
                        if (keyword.Section != null)
                            writer.WriteLine(keyword.Section);
                        writer.WriteLine(keyword.Text);
                        if (!String.IsNullOrEmpty(keyword.Urls))
                        {
                            foreach (string url in keyword.Urls.Split('\n'))
                                writer.WriteLine("\t" + url);
                        }
                    }
                }
            }
        }
    }
}
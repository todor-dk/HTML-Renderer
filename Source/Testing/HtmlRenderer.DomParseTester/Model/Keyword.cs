using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.Model
{
    public class Keyword
    {
        private string Section;

        public string Text { get; set; }

        public ObservableCollection<string> Urls { get; private set; }

        public bool IsResolved
        {
            get { return this.Urls.Count == 0; }
        }

        public Keyword(string text, string[] urls = null)
        {
            this.Text = text;
            this.Urls = new ObservableCollection<string>(urls ?? Array.Empty<string>());
        }
        
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
                                keyword.Urls.Add(trimmed);

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
            using (Stream file = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamWriter writer = new StreamWriter(file, System.Text.Encoding.UTF8))
                {
                    foreach (Keyword keyword in words)
                    {
                        if (keyword.Section != null)
                            writer.WriteLine(keyword.Section);
                        writer.WriteLine(keyword.Text);
                        if (keyword.Urls.Count != 0)
                        {
                            foreach (string url in keyword.Urls)
                                writer.WriteLine("\t" + url);
                        }
                    }
                }
            }
        }
    }
}

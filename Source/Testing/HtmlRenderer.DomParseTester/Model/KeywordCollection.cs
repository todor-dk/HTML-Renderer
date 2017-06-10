using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.Model
{
    public class KeywordCollection
    {
        public string Path { get; private set; }

        public Keyword[] Keywords { get; private set; }

        public KeywordCollection(string path)
        {
            this.Path = path;
            this.Keywords = Keyword.LoadKeywords(path);
        }
    }
}

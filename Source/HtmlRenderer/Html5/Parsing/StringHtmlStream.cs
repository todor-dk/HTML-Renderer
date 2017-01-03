using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    public sealed class StringHtmlStream : HtmlStream
    {
        private readonly string Html;

        private int Index;

        private readonly int Limit;

        public StringHtmlStream(string html)
        {
            Contract.RequiresNotNull(html, nameof(html));
            this.Html = html;
            this.Index = 0;
            this.Limit = html.Length;
        }

        public override char ReadChar()
        {
            if (this.Index >= this.Limit)
                return Characters.EOF;
            char ch = this.Html[this.Index];
            this.Index++;
            if (ch == Characters.EOF)
                ch = Characters.ReplacementCharacter; // U+FFFF is not allowed character
            return ch;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    public abstract class ParsingContext
    {
        public ParsingContext(string url)
        {
            Contract.RequiresNotEmptyOrWhiteSpace(url, nameof(url));
            this.Url = url;
        }

        public string Url { get; private set; }

        // http://www.w3.org/TR/html51/semantics-embedded-content.html#iframe-iframe-srcdoc-document
        public bool IsIFrameSource
        {
            get { return false; }
        }

        // http://www.w3.org/TR/html51/syntax.html#parsing-html-fragments
        public bool IsFragmentParsing
        {
            get { return false; }
        }

        public Element FragmentContextElement
        {
            get { return null; }
        }

        internal abstract DomFactory GetDomFactory();
    }
}

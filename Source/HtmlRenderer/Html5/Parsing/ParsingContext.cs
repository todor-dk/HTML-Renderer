using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    public class ParsingContext
    {
        public Document Document
        {
            get { throw new NotImplementedException(); }
        }

        //public Element DocumentElement
        //{
        //    get { throw new NotImplementedException(); }
        //}

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

        internal DomFactory GetDomFactory()
        {
            throw new NotImplementedException();
        }
    }
}

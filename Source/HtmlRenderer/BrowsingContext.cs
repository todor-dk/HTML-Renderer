using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer
{
    public class BrowsingContext
    {
        // See: http://www.w3.org/TR/html5/webappapis.html#enabling-and-disabling-scripting
        public bool IsScriptingEnabled
        {
            get { return false; }
        }

        public Dom.Document CreateHtmlDocument(string baseUri, string characterSet)
        {
            return new Internal.DomImplementation.HtmlDocument(this, baseUri, characterSet);
        }

        public Html5.Parsing.ParsingContext GetDocumentParsingContext(string url)
        {
            return new Internal.DomImplementation.DocumentParsingContext(this, url);
        }
    }
}

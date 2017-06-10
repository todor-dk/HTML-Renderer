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

        public Dom.Document CreateDocument(string baseUri)
        {
            return new Internal.DomImplementation.Document(this, baseUri);
        }
    }
}

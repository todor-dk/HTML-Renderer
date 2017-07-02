using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Html5;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class HtmlDocument : Document
    {
        public override string ContentType
        {
            get { return "text/html"; }
        }

        public HtmlDocument(BrowsingContext browsingContext, string location, string characterSet)
            : base(browsingContext, location, characterSet)
        {
        }

        protected override Element CreateElement(string namespaceUri, string prefix, string localName)
        {
            if ((namespaceUri == Namespaces.Html) && (prefix == null))
            {
                return new HtmlUnknownElement(this, localName);
            }

            return new ForeignElement(this, namespaceUri, prefix, localName);
        }
    }
}

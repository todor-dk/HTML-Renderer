using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;
using Scientia.HtmlRenderer.Html5.Parsing;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal class DocumentParsingContext : ParsingContext
    {
        private readonly BrowsingContext BrowsingContext;

        public DocumentParsingContext(BrowsingContext browsingContext, string url)
            : base(url)
        {
            Contract.RequiresNotNull(browsingContext, nameof(browsingContext));

            this.BrowsingContext = browsingContext;
        }

        internal override DomFactory GetDomFactory()
        {
            return new InternalDomFactory(this.BrowsingContext);
        }

        private class InternalDomFactory : DomFactory
        {
            private readonly BrowsingContext BrowsingContext;

            public InternalDomFactory(BrowsingContext browsingContext)
            {
                Contract.RequiresNotNull(browsingContext, nameof(browsingContext));

                this.BrowsingContext = browsingContext;
            }

            public override Dom.Document CreateDocument(string baseUri, string characterSet)
            {
                return this.BrowsingContext.CreateHtmlDocument(baseUri, characterSet);
            }

            public override void SetQuirksMode(Dom.Document document, QuirksMode mode)
            {
                Document doc = (Document)document;
                doc.QuirksMode = mode;
            }
        }
    }
}

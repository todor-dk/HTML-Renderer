using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal class DomImplementation : Dom.DomImplementation
    {
        public Dom.DocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId)
        {
            throw new NotImplementedException();
        }

        public Dom.Document CreateHtmlDocument(string title = null)
        {
            throw new NotImplementedException();
        }
    }
}

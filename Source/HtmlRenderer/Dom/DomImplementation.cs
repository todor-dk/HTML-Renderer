using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-domimplementation
    /// </summary>
    public interface DomImplementation
    {
        DocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId);

        Document CreateHtmlDocument(string title = null);
    }
}

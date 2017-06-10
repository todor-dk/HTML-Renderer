using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Html5.Parsing;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    internal abstract class DomFactory
    {
        public abstract void SetQuirksMode(QuirksMode mode);

        /// <summary>
        /// Append a DocumentType node to the Document node.
        /// Associate the DocumentType node with the Document object so that it is
        /// returned as the value of the doctype attribute of the Document object.
        /// </summary>
        public abstract void AppendDocType(string name, string publicIdentifier, string systemIdentifier);

        public abstract Element CreateElement(Document document, string namespaceUri, string tagName, Html5.Parsing.Attribute[] attributes);

        public abstract void AppendElement(Element parentElement, Element element);

        public abstract void InsertElementBefore(Element parentElement, Element element, Element referenceElement);

        public void InsertChildNode(Element parent, Node child, bool unparentIfNeeded)
        {
            
        }

        public void AssociateWithForm(Element element, Element form)
        {
            // Nothing here.
        }

        public void InvokeResetAlgorithm(Element element)
        {
            // Nothing here.

            // See: http://www.w3.org/TR/html51/sec-forms.html#reset-algorithm

            // This resets <input>, <keygen>, <output>, <select> and <textarea>.
            // The alg. is element specific. We don't need to implement this, because we have no interactivity.
        }

        public void StopParsing()
        {
            // Nothing here.

            // See: http://www.w3.org/TR/html51/syntax.html#stopped

            // There are a lot of steps to make the doc interactive. But this is irrelevant for us!
        }

    }
}

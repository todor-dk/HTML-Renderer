using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Html5.Parsing;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    internal class DomFactory
    {
        public readonly DomParser Parser;

        public DomFactory(DomParser parser)
        {
            Contract.RequiresNotNull(parser, nameof(parser));
            this.Parser = parser;
        }

        public void AppendDocType(string name, string publicIdentifier, string systemIdentifier)
        {
            // Append a DocumentType node to the Document node. Associate
            // the DocumentType node with the Document object so that it is returned as the value of the doctype
            // attribute of the Document object.
        }

        public void SetQuirksMode(QuirksMode mode)
        {
        }

        public enum QuirksMode
        {
            On,

            Limited,

            Off
        }

        public Element CreateElement(string name, Html5.Parsing.Attribute[] attributes, bool addToStack)
        {
            // See: http://www.w3.org/TR/html5/syntax.html#create-an-element-for-the-token
            return null;
        }

        public void InsertComment(string data)
        {
            // See: http://www.w3.org/TR/html5/syntax.html#insert-a-comment
        }

        public Element InsertHtmlElement(string name, Html5.Parsing.Attribute[] attributes)
        {
            // See: http://www.w3.org/TR/html5/syntax.html#insert-an-html-element
            return null;
        }

        public void InsertCharacter(char ch)
        {

        }
    }
}

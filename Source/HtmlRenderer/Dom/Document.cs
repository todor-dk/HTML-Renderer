using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-document
    public interface Document : Node, NonElementParentNode, ParentNode
    {
        QuirksMode QuirksMode { get; }

        DomImplementation DomImplementation { get; }

        string Url { get; }

        string DocumentUri { get; }

        string Origin { get; }

        string CompatMode { get; }

        string CharacterSet { get; }

        string ContentType { get; }

        DocumentType DocType { get; }

        Element DocumentElement { get; }

        HtmlCollection GetElementsByTagName(string localName);

        HtmlCollection GetElementsByTagNameNS(string @namespace, string localName);

        HtmlCollection GetElementsByClassName(string className);

        Element CreateElement(string localName);

        Element CreateElementNS(string @namespace, string qualifiedName);

        DocumentFragment CreateDocumentFragment();

        Text CreateTextNode(string data);

        Comment CreateComment(string data);

        ProcessingInstruction CreateProcessingInstruction(string target, string data);

        Node ImportNode(Node node, bool deep = false);

        Node AdoptNode(Node node);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-document
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1302 // Interface names must begin with I
    public interface Document : Node, NonElementParentNode, ParentNode
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        QuirksMode QuirksMode { get; }

        /// <summary>
        /// Returns the <see cref="DomImplementation"/> associated with this document.
        /// </summary>
        DomImplementation Implementation { get; }

        /// <summary>
        /// Returns the document location URL.
        /// This is the same as <see cref="DocumentUri"/>.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Returns the document location URL.
        /// This is the same as <see cref="Url"/>.
        /// </summary>
        string DocumentUri { get; }

        /// <summary>
        /// Not Implemented. See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-document-origin
        /// </summary>
        string Origin { get; }

        /// <summary>
        /// Return "BackCompat" if the document is in quirks mode, and "CSS1Compat" otherwise.
        /// </summary>
        string CompatMode { get; }

        /// <summary>
        /// Returns the character encoding of the current document.
        /// </summary>
        string CharacterSet { get; }

        /// <summary>
        /// Not Implemented. See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-document-contenttype
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Returns the child of the document that is a <see cref="DocumentType"/>, and null otherwise.
        /// </summary>
        DocumentType DocType { get; }

        /// <summary>
        /// Returns the Element that is the root element of the document (for example, the <html> element for HTML documents).
        /// </summary>
        Element DocumentElement { get; }

        HtmlCollection GetElementsByTagName(string localName);

        HtmlCollection GetElementsByTagNameNS(string @namespace, string localName);

        HtmlCollection GetElementsByClassName(string className);

        /// <summary>
        /// Creates the HTML element specified by <paramref name="localName"/>.
        /// </summary>
        /// <param name="localName">Specifies the type of element to be created. This is converted to lowercase.</param>
        /// <returns>The new Element.</returns>
        Element CreateElement(string localName);

        /// <summary>
        /// Creates an element with the specified namespace URI and qualified name.
        /// </summary>
        /// <param name="namespace">The namespace URI to associate with the element.</param>
        /// <param name="qualifiedName">A string that specifies the type of element to be created.</param>
        /// <returns>The new Element.</returns>
        Element CreateElementNS(string @namespace, string qualifiedName);

        /// <summary>
        /// Creates a new empty DocumentFragment.
        /// </summary>
        /// <returns>A new empty DocumentFragment.</returns>
        DocumentFragment CreateDocumentFragment();

        /// <summary>
        /// Creates a new Text node.
        /// </summary>
        /// <param name="data">Data to be put in the text node.</param>
        /// <returns>A new Text node with the given data.</returns>
        Text CreateTextNode(string data);

        /// <summary>
        /// Creates a new comment node, and returns it.
        /// </summary>
        /// <param name="data">Data to be added to the Comment.</param>
        /// <returns>A new Comment node with the given data.</returns>
        Comment CreateComment(string data);

        /// <summary>
        /// Creates a new processing instruction node, and returns it.
        /// </summary>
        /// <param name="target">The target part of the processing instruction node.</param>
        /// <param name="data">The data to be added to the data within the node.</param>
        /// <returns>A new processing instruction node.</returns>
        ProcessingInstruction CreateProcessingInstruction(string target, string data);

        /// <summary>
        /// Creates a copy of a node from an external document that can be inserted into the current document.
        /// </summary>
        /// <param name="node">The node from another document to be imported.</param>
        /// <param name="deep"> boolean, indicating whether the descendants of the imported node need to be imported.</param>
        /// <returns>
        /// The new node that is imported into the document. The new node's parentNode is null,
        /// since it has not yet been inserted into the document tree.
        /// </returns>
        Node ImportNode(Node node, bool deep = false);

        /// <summary>
        /// Adopts a node. The node and its subtree is removed from the document it's in (if any), and its ownerDocument
        /// is changed to the current document. The node can then be inserted into the current document.
        /// </summary>
        /// <param name="node">A node from another document to be adopted.</param>
        /// <returns>
        /// The adopted node that now has this document as its <see cref="Node.OwnerDocument"/>. The node's
        /// <see cref="Node.ParentNode"/> is null, since it has not yet been inserted into the document tree.
        /// Note that the return value is the same as the given <paramref name="node"/>.
        /// </returns>
        Node AdoptNode(Node node);
    }
}

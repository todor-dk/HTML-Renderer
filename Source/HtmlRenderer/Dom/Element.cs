using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-element
    public interface Element : Node, ParentNode, NonDocumentTypeChildNode, ChildNode
    {
        /// <summary>
        /// Returns the namespace URI of the element, or null if the element is not in a namespace.
        /// </summary>
        string NamespaceUri { get; }

        /// <summary>
        /// Returns the namespace prefix of the specified element, or null if no prefix is specified.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Returns the local part of the qualified name of an element.
        /// </summary>
        string LocalName { get; }

        /// <summary>
        /// Returns the qualified name of the element.
        /// For HTML elements in HTML documents, <see cref="TagName"/> returns the element name in the uppercase.
        /// </summary>
        string TagName { get; }

        /// <summary>
        /// Get or set the element's identifier, reflecting the <see cref="Attr"/> named "id".
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Get or set the value of the "class" attribute of this element.
        /// </summary>
        string ClassName { get; set; }

        DomTokenList ClassList { get; }

        AttrCollection Attributes { get; }

        string GetAttribute(string name);

        string GetAttributeNS(string @namespace, string localName);

        void SetAttribute(string name, string value);

        void SetAttributeNS(string @namespace, string name, string value);

        void RemoveAttribute(string name);

        void RemoveAttributeNS(string @namespace, string localName);

        bool HasAttribute(string name);

        bool HasAttributeNS(string @namespace, string localName);

        HtmlCollection GetElementsByTagName(string localName);

        HtmlCollection GetElementsByTagNameNS(string @namespace, string localName);

        HtmlCollection GetElementsByClassName(string classNames);
    }
}

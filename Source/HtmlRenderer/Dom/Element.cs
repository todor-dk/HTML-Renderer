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
        /// <remarks>
        /// If the attribute is missing, an empty string is returned.
        /// Setting the value of this property either overwrites the existing "id" attribute or adds a new attribute.
        /// </remarks>
        string Id { get; set; }

        /// <summary>
        /// Get or set the value of the "class" attribute of this element.
        /// </summary>
        /// <remarks>
        /// If the attribute is missing, an empty string is returned.
        /// Setting the value of this property either overwrites the existing "class" attribute or adds a new attribute.
        /// </remarks>
        string ClassName { get; set; }

        /// <summary>
        /// Return a <see cref="DomTokenList"/> object representing the <see cref="ClassName"/> value, i.e. the element's "class" attribute.
        /// </summary>
        DomTokenList ClassList { get; }

        /// <summary>
        /// Returns a live collection of all attribute nodes registered to this node.
        /// </summary>
        AttrCollection Attributes { get; }

        /// <summary>
        /// Returns the value of a specified attribute on the element or null if no attribute exists.
        /// </summary>
        /// <param name="name">The name of the attribute whose value you want to get</param>
        /// <returns>The value of the attribute named <paramref name="name"/> or null if no attribute exists with the given name.</returns>
        string GetAttribute(string name);

        /// <summary>
        /// Returns the value of the attribute with the specified namespace and name or null if no attribute exists.
        /// </summary>
        /// <param name="namespace">The namespace in which to look for the specified attribute.</param>
        /// <param name="localName">The local name of the attribute to look for.</param>
        /// <returns>The string value of the specified attribute. If the attribute doesn't exist, the result is null.</returns>
        string GetAttributeNS(string @namespace, string localName);

        /// <summary>
        /// Sets the value of an attribute on the specified element. If the attribute already exists,
        /// the value is updated; otherwise a new attribute is added with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the attribute whose value is to be set.</param>
        /// <param name="value">The value to assign to the attribute.</param>
        void SetAttribute(string name, string value);

        /// <summary>
        /// Adds a new attribute or changes the value of an attribute with the given namespace and name.
        /// </summary>
        /// <param name="namespace">The namespace of the attribute.</param>
        /// <param name="name">The attribute to be set.</param>
        /// <param name="value">Value of the new attribute.</param>
        void SetAttributeNS(string @namespace, string name, string value);

        /// <summary>
        /// Removes an attribute from the specified element.
        /// </summary>
        /// <param name="name">The name of the attribute to be removed from element.</param>
        void RemoveAttribute(string name);

        /// <summary>
        /// Removes the specified attribute from this element.
        /// </summary>
        /// <param name="namespace">The namespace of the attribute.</param>
        /// <param name="localName">The name of the attribute to be removed from the current node.</param>
        void RemoveAttributeNS(string @namespace, string localName);

        /// <summary>
        /// Indicates whether this element has the specified attribute or not.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>True if this element has the specified attribute, otherwise false.</returns>
        bool HasAttribute(string name);

        /// <summary>
        /// Indicates whether this element has the specified attribute or not.
        /// </summary>
        /// <param name="namespace">The namespace of the attribute.</param>
        /// <param name="localName">The local name of the attribute.</param>
        /// <returns>True if this element has the specified attribute, otherwise false.</returns>
        bool HasAttributeNS(string @namespace, string localName);

        HtmlCollection GetElementsByTagName(string localName);

        HtmlCollection GetElementsByTagNameNS(string @namespace, string localName);

        HtmlCollection GetElementsByClassName(string classNames);
    }
}

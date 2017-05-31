using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-node
    public interface Node
    {
        /// <summary>
        /// Returns the type of node.
        /// </summary>
        NodeType NodeType { get; }

        /// <summary>
        /// Returns a string appropriate for the type of node.
        /// </summary>
        string NodeName { get; }

        /// <summary>
        /// Returns the absolute base URL of a node.
        /// </summary>
        /// <remarks>
        /// The base URL of a document defaults to the document's address.
        /// The base URL of an element in HTML normally equals the base URL of the document the node is in.
        /// <para/>
        /// If the document contains xml:base attributes (which you shouldn't do in HTML documents),
        /// the element.baseURI takes the xml:base attributes of element's parents into account when
        /// computing the base URL.
        /// NB: We do not support xml:base
        /// </remarks>
        string BaseUri { get; }

        /// <summary>
        /// Returns the top-level document object for this node.
        /// If this property is used on a node that is itself a <see cref="Document"/>, the result is null.
        /// </summary>
        Document OwnerDocument { get; }

        /// <summary>
        /// Returns the parent of the specified node in the DOM tree. The parent of an <see cref="Element"/>
        /// is an <see cref="Element"/> node, a <see cref="Document"/> node, <see cref="DocumentFragment"/> node.
        /// <see cref="Document"/> and <see cref="DocumentFragment"/> nodes can never have a parent,
        /// so ParentNode will always return null.
        /// </summary>
        Node ParentNode { get; }

        /// <summary>
        /// Returns the node's parent <see cref="Element"/>, or null if the node either has no parent,
        /// or its parent is of a type different than Element.
        /// </summary>
        Element ParentElement { get; }

        /// <summary>
        /// Returns a bool value indicating whether the current Node has child nodes or not.
        /// </summary>
        /// <returns>True of the current node has children, otherwise false.</returns>
        bool HasChildren();

        /// <summary>
        /// Returns a live collection of child nodes of the given element where the first child node is assigned index 0.
        /// </summary>
        /// <remarks>
        /// <see cref="ChildNodes"/> includes all direct child nodes, including non-element nodes like text and comment nodes.
        /// To get a collection of only elements, use <see cref="ParentNode.Children"/> instead.
        /// </remarks>
        NodeList ChildNodes { get; }

        /// <summary>
        /// Returns the node's first child in the tree, or null if the node is childless. If the node is a Document,
        /// it returns the first node in the list of its direct children.
        /// </summary>
        Node FirstChild { get; }

        /// <summary>
        /// Returns the last child of the node. If its parent is an element, then the child is generally an element node,
        /// a text node, or a comment node. It returns null if there are no child elements.
        /// </summary>
        Node LastChild { get; }

        /// <summary>
        /// Returns the node immediately preceding the specified one in its parent's <see cref="ChildNodes"/> list,
        /// or null if the specified node is the first in that list.
        /// </summary>
        Node PreviousSibling { get; }

        /// <summary>
        /// Returns the node immediately following the specified one in its parent's <see cref="ChildNodes"/> list,
        /// or null if the specified node is the last node in that list.
        /// </summary>
        Node NextSibling { get; }

        /// <summary>
        /// The value of this node, depending on its type.
        /// For <see cref="Text"/> and <see cref="Comment"/>, this is the textual <see cref="CharacterData.Data"/> of the node.
        /// For other nodes, this is null.
        /// </summary>
        string NodeValue { get; set; }

        /// <summary>
        /// The text content of a node and its descendants.
        /// </summary>
        string TextContent { get; set; }

        void Normalize();

        Node CloneNode(bool deep = false);

        DocumentPosition CompareDocumentPosition(Node other);

        bool Contains(Node other);

        string LookupPrefix(string @namespace);

        string LookupNamespaceUri(string prefix);

        string IsDefaultNamespace(string @namespace);

        Node InsertBefore(Node node, Node child);

        Node AppendChild(Node node);

        Node ReplaceChild(Node node, Node child);

        Node RemoveChild(Node child);
    }
}

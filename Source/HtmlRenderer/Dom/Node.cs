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

        /// <summary>
        /// Puts the node and all of its sub-tree into a "normalized" form. In a normalized sub-tree,
        /// no text nodes in the sub-tree are empty and there are no adjacent text nodes.
        /// </summary>
        void Normalize();

        /// <summary>
        /// Returns a duplicate of this node.
        /// </summary>
        /// <param name="deep">True if the children of the node should also be cloned, or false to clone only the specified node.</param>
        /// <returns>A new node that is a clone this node.</returns>
        Node CloneNode(bool deep = false);

        /// <summary>
        /// Compares the position of this node against a given node in any other document.
        /// </summary>
        /// <param name="other">Another node to compare to this node.</param>
        /// <returns>Returns a bitmask indicating the position of other relative to node.</returns>
        DocumentPosition CompareDocumentPosition(Node other);

        /// <summary>
        /// Determines whether this node is a descendant of a given node or not.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        /// True if <paramref name="other"/> is an inclusive descendant of this node, false otherwise (including when other is null).
        /// </returns>
        bool Contains(Node other);

        /// <summary>
        /// Returns a string containing the prefix for a given namespace URI, if present, and null if not.
        /// When multiple prefixes are possible, the result is random.
        /// </summary>
        /// <param name="namespace">The requested namespace URI.</param>
        /// <returns>Returns a string containing the prefix for a given namespace URI, if present, and null if not.</returns>
        string LookupPrefix(string @namespace);

        /// <summary>
        /// Takes a <paramref name="prefix"/> and returns the namespace URI associated with it on this node if found (and null if not).
        /// Supplying null for the prefix will return the default namespace.
        /// </summary>
        /// <param name="prefix">The prefix to look for. If this parameter is null, the method will return the default namespace URI if any.</param>
        /// <returns>Returns the associated namespace URI or null if none is found.</returns>
        string LookupNamespaceUri(string prefix);

        /// <summary>
        /// Determines if the given namespace URI is the default namespace on this node or false if not.
        /// </summary>
        /// <param name="namespace">The namespace against which the node will be checked.</param>
        /// <returns>True if the namespace is the default namespace on this node or false if not.</returns>
        bool IsDefaultNamespace(string @namespace);

        /// <summary>
        /// Inserts the specified <paramref name="newChild"/> before the reference node as a child of the current node.
        /// </summary>
        /// <param name="newChild">New node to be added as a child to this node.</param>
        /// <param name="referenceChild">Reference child node. If referenceNode is null, the newNode is inserted at the end of the list of child nodes.</param>
        /// <returns>The inserted node.</returns>
        Node InsertBefore(Node newChild, Node referenceChild);

        /// <summary>
        /// Adds <paramref name="node"/> to the end of the list of children of this node. If the given child is
        /// an existing node in the document, AppendChild moves it from its current position to the new position
        /// (there is no requirement to remove the node from its parent node before appending it to some other node).
        /// </summary>
        /// <param name="node">Node to be added.</param>
        /// <returns>The appended child.</returns>
        Node AppendChild(Node node);

        /// <summary>
        /// Replaces one child node of the specified node with another.
        /// </summary>
        /// <param name="newChild">The new node to replace existingChild. If it already exists in the DOM, it is first removed.</param>
        /// <param name="existingChild">The existing child to be replaced</param>
        /// <returns>The replaced node. This is the same node as oldChild.</returns>
        Node ReplaceChild(Node newChild, Node existingChild);

        /// <summary>
        /// Removes a child node from the DOM. Returns removed node.
        /// </summary>
        /// <param name="child">Child node to be removed from the DOM.</param>
        /// <returns>Returns the removed node.</returns>
        /// <exception cref="NotFoundException">If <paramref name="child"/>'s <see cref="Node.ParentNode"/> is not this node.</exception>
        Node RemoveChild(Node child);
    }
}

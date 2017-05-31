using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal abstract class Node : Dom.Node
    {
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
        public virtual string BaseUri
        {
            get { return this.OwnerDocument?.BaseUri; }
        }

        /// <summary>
        /// Returns a live collection of child nodes of the given element where the first child node is assigned index 0.
        /// </summary>
        /// <remarks>
        /// <see cref="ChildNodes"/> includes all direct child nodes, including non-element nodes like text and comment nodes.
        /// To get a collection of only elements, use <see cref="ParentNode.Children"/> instead.
        /// </remarks>
        public virtual NodeList ChildNodes
        {
            get { return EmptyCollection.Current; }
        }

        /// <summary>
        /// Returns the node's first child in the tree, or null if the node is childless. If the node is a Document,
        /// it returns the first node in the list of its direct children.
        /// </summary>
        public virtual Dom.Node FirstChild
        {
            get { return null; }
        }

        /// <summary>
        /// Returns the last child of the node. If its parent is an element, then the child is generally an element node,
        /// a text node, or a comment node. It returns null if there are no child elements.
        /// </summary>
        public virtual Dom.Node LastChild
        {
            get { return null; }
        }

        /// <summary>
        /// Returns the node immediately following the specified one in its parent's <see cref="ChildNodes"/> list,
        /// or null if the specified node is the last node in that list.
        /// </summary>
        public Dom.Node NextSibling
        {
            get { return this._ParentNode?.GetNode(this.NodeCollectionIndex + 1); }
        }

        /// <summary>
        /// Returns a string appropriate for the type of node.
        /// </summary>
        public abstract string NodeName { get; }

        /// <summary>
        /// Returns the type of node.
        /// </summary>
        public abstract NodeType NodeType { get; }

        /// <summary>
        /// The value of this node, depending on its type.
        /// For <see cref="Text"/> and <see cref="Comment"/>, this is the textual <see cref="CharacterData.Data"/> of the node.
        /// For other nodes, this is null.
        /// </summary>
        public virtual string NodeValue
        {
            get { return null; }
            set { /* do nothing */ }
        }

        /// <summary>
        /// Returns the top-level document object for this node.
        /// If this property is used on a node that is itself a <see cref="Document"/>, the result is null.
        /// </summary>
        public Dom.Document OwnerDocument { get; private set; }

        /// <summary>
        /// Returns the node's parent <see cref="Element"/>, or null if the node either has no parent,
        /// or its parent is of a type different than Element.
        /// </summary>
        public Dom.Element ParentElement
        {
            get { return this._ParentNode as Dom.Element; }
        }

        public void SetParent(ParentNode parent, int nodeCollectionIndex)
        {
            this._ParentNode = parent;
            this.NodeCollectionIndex = (parent != null) ? nodeCollectionIndex : Node.NotAttachedToParentIndex;
        }

        private ParentNode _ParentNode;

        private int NodeCollectionIndex;

        private const int NotAttachedToParentIndex = -2;

        /// <summary>
        /// Returns the parent of the specified node in the DOM tree. The parent of an <see cref="Element"/>
        /// is an <see cref="Element"/> node, a <see cref="Document"/> node, <see cref="DocumentFragment"/> node.
        /// <see cref="Document"/> and <see cref="DocumentFragment"/> nodes can never have a parent,
        /// so ParentNode will always return null.
        /// </summary>
        public Dom.Node ParentNode
        {
            get { return this._ParentNode; }
        }

        /// <summary>
        /// Returns the node immediately preceding the specified one in its parent's <see cref="ChildNodes"/> list,
        /// or null if the specified node is the first in that list.
        /// </summary>
        public Dom.Node PreviousSibling
        {
            get { return this._ParentNode?.GetNode(this.NodeCollectionIndex - 1); }
        }

        /// <summary>
        /// The text content of a node and its descendants.
        /// </summary>
        public virtual string TextContent
        {
            get { return null; }
            set { /* do nothing */ }
        }

        protected Node(Document document)
        {
            if (this is Document)
                Contract.RequiresNull(document, nameof(document));
            else
                Contract.RequiresNotNull(document, nameof(document));

            this.OwnerDocument = document;
        }

        public Dom.Node AppendChild(Dom.Node node)
        {
            throw new NotImplementedException();
        }

        public Dom.Node CloneNode(bool deep = false)
        {
            throw new NotImplementedException();
        }

        public DocumentPosition CompareDocumentPosition(Dom.Node other)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Dom.Node other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a bool value indicating whether the current Node has child nodes or not.
        /// </summary>
        /// <returns>True of the current node has children, otherwise false.</returns>
        public virtual bool HasChildren()
        {
            return false;
        }

        public Dom.Node InsertBefore(Dom.Node node, Dom.Node child)
        {
            throw new NotImplementedException();
        }

        public string IsDefaultNamespace(string @namespace)
        {
            throw new NotImplementedException();
        }

        public string LookupNamespaceUri(string prefix)
        {
            throw new NotImplementedException();
        }

        public string LookupPrefix(string @namespace)
        {
            throw new NotImplementedException();
        }

        public void Normalize()
        {
            throw new NotImplementedException();
        }

        public Dom.Node RemoveChild(Dom.Node child)
        {
            throw new NotImplementedException();
        }

        public Dom.Node ReplaceChild(Dom.Node node, Dom.Node child)
        {
            throw new NotImplementedException();
        }
    }
}

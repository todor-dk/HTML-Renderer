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
        internal ParentNode _ParentNode { get; private set; }

        internal int NodeCollectionIndex { get; private set; }

        private const int NotAttachedToParentIndex = -2;

        protected Node(Document document)
        {
            if (this is Document)
                Contract.RequiresNull(document, nameof(document));
            else
                Contract.RequiresNotNull(document, nameof(document));

            this._ParentNode = null;
            this.NodeCollectionIndex = Node.NotAttachedToParentIndex;
            this.OwnerDocument = document;
        }

        #region Node implementation

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
            get { return this.Document?.BaseUri; }
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

        /// <summary>
        /// Adds <paramref name="node"/> to the end of the list of children of this node. If the given child is
        /// an existing node in the document, AppendChild moves it from its current position to the new position
        /// (there is no requirement to remove the node from its parent node before appending it to some other node).
        /// </summary>
        /// <param name="node">Node to be added.</param>
        /// <returns>The appended child.</returns>
        public virtual Dom.Node AppendChild(Dom.Node node)
        {
            throw new Dom.Exceptions.HierarchyRequestException();
        }

        /// <summary>
        /// Returns a duplicate of this node.
        /// </summary>
        /// <param name="deep">True if the children of the node should also be cloned, or false to clone only the specified node.</param>
        /// <returns>A new node that is a clone this node.</returns>
        public abstract Dom.Node CloneNode(bool deep = false);

        /// <summary>
        /// Compares the position of this node against a given node in any other document.
        /// </summary>
        /// <param name="other">Another node to compare to this node.</param>
        /// <returns>Returns a bitmask indicating the position of other relative to node.</returns>
        public DocumentPosition CompareDocumentPosition(Dom.Node other)
        {
            Contract.RequiresNotNull(other, nameof(other));

            // 1. Let reference be the context object.
            Node reference = this;

            // 2. If other and reference are the same object, return zero.
            if (other == reference)
                return DocumentPosition.None;

            // 3. If other and reference are not in the same tree, return the result
            //    of adding DOCUMENT_POSITION_DISCONNECTED, DOCUMENT_POSITION_IMPLEMENTATION_SPECIFIC,
            //    and either DOCUMENT_POSITION_PRECEDING or DOCUMENT_POSITION_FOLLOWING, with the
            //    constraint that this is to be consistent, together.
            // NOTE: Whether to return DOCUMENT_POSITION_PRECEDING or DOCUMENT_POSITION_FOLLOWING is typically
            //       implemented via pointer comparison. In JavaScript implementations Math.random() can be used.
            if (((Node)other).RootNode != reference.RootNode)
                return DocumentPosition.Disconnected | DocumentPosition.ImplementationSpecific | DocumentPosition.Preceding;

            // 4. If other is an ancestor of reference, return the result of adding DOCUMENT_POSITION_CONTAINS
            //    to DOCUMENT_POSITION_PRECEDING.
            Node node = reference;
            while (node != null)
            {
                if (node == other)
                    return DocumentPosition.Contains | DocumentPosition.Preceding;

                node = node._ParentNode;
            }

            // 5. If other is a descendant of reference, return the result of adding
            //    DOCUMENT_POSITION_CONTAINED_BY to DOCUMENT_POSITION_FOLLOWING.
            node = (Node)other;
            while (node != null)
            {
                if (node == reference)
                    return DocumentPosition.ContainedBy | DocumentPosition.Following;

                node = node._ParentNode;
            }

            // 6. If other is preceding reference return DOCUMENT_POSITION_PRECEDING.
            // 7. Return DOCUMENT_POSITION_FOLLOWING.

            // ASSUMPTION: They must be siblings.
            foreach (Dom.Node sibling in reference.ParentNode.ChildNodes)
            {
                if (other == sibling)
                    return DocumentPosition.Preceding;
                if (reference == sibling)
                    return DocumentPosition.Following;
            }

            throw new InvalidOperationException("Should not happen");
        }

        /// <summary>
        /// Determines whether this node is a descendant of a given node or not.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        /// True if <paramref name="other"/> is an inclusive descendant of this node, false otherwise (including when other is null).
        /// </returns>
        public bool Contains(Dom.Node other)
        {
            if (other == null)
                return false;

            Node node = this;
            while (node != null)
            {
                if (node == other)
                    return true;

                node = node._ParentNode;
            }

            return false;
        }

        /// <summary>
        /// Returns a bool value indicating whether the current Node has child nodes or not.
        /// </summary>
        /// <returns>True of the current node has children, otherwise false.</returns>
        public virtual bool HasChildren()
        {
            return false;
        }

        /// <summary>
        /// Inserts the specified <paramref name="newChild"/> before the reference node as a child of the current node.
        /// </summary>
        /// <param name="newChild">New node to be added as a child to this node.</param>
        /// <param name="referenceChild">Reference child node. If referenceNode is null, the newNode is inserted at the end of the list of child nodes.</param>
        /// <returns>The inserted node.</returns>
        public virtual Dom.Node InsertBefore(Dom.Node newChild, Dom.Node referenceChild)
        {
            throw new Dom.Exceptions.HierarchyRequestException();
        }

        /// <summary>
        /// Determines if the given namespace URI is the default namespace on this node or false if not.
        /// </summary>
        /// <param name="namespace">The namespace against which the node will be checked.</param>
        /// <returns>True if the namespace is the default namespace on this node or false if not.</returns>
        public bool IsDefaultNamespace(string @namespace)
        {
            // 1. If namespace is the empty string, set it to null.
            if (@namespace == "")
                @namespace = null;

            // 2. Let defaultNamespace be the result of running locate a namespace for the context object using null.
            string defaultNamespace = this.LocateNamespaceUri(null);

            // 3. Return true if defaultNamespace is the same as namespace, and false otherwise.
            return defaultNamespace == @namespace;
        }

        /// <summary>
        /// Takes a <paramref name="prefix"/> and returns the namespace URI associated with it on this node if found (and null if not).
        /// Supplying null for the prefix will return the default namespace.
        /// </summary>
        /// <param name="prefix">The prefix to look for. If this parameter is null, the method will return the default namespace URI if any.</param>
        /// <returns>Returns the associated namespace URI or null if none is found.</returns>
        public string LookupNamespaceUri(string prefix)
        {
            // 1. If prefix is the empty string, set it to null.
            if (prefix == "")
                prefix = null;

            // 2. Return the result of running locate a namespace for the context object using prefix.
            return this.LocateNamespaceUri(prefix);
        }

        /// <summary>
        /// Returns a string containing the prefix for a given namespace URI, if present, and null if not.
        /// When multiple prefixes are possible, the result is random.
        /// </summary>
        /// <param name="namespace">The requested namespace URI.</param>
        /// <returns>Returns a string containing the prefix for a given namespace URI, if present, and null if not.</returns>
        public string LookupPrefix(string @namespace)
        {
            // 1. If namespace is null or the empty string, return null.
            if (String.IsNullOrEmpty(@namespace))
                return null;

            // 2. Otherwise it depends on the context object:

            // * Element
            if (this is Dom.Element)
            {
                // Return the result of locating a namespace prefix for the node using namespace.
                return ((Dom.Element)this).LocateNamespacePrefix(@namespace);
            }

            // * Document
            if (this is Document)
            {
                // Return the result of locating a namespace prefix for its document element, if that is not null, and null otherwise.
                return ((Document)this).DocumentElement?.LocateNamespacePrefix(@namespace);
            }

            // * DocumentType
            // * DocumentFragment
            if ((this is DocumentType) || (this is DocumentFragment))
            {
                // Return null.
                return null;
            }

            // * Any other node
            // Return the result of locating a namespace prefix for its parent element, or if that is null, null.
            return this.ParentElement?.LocateNamespacePrefix(@namespace);
        }

        /// <summary>
        /// Puts the node and all of its sub-tree into a "normalized" form. In a normalized sub-tree,
        /// no text nodes in the sub-tree are empty and there are no adjacent text nodes.
        /// </summary>
        public virtual void Normalize()
        {
            // Do nothing. Only nodes with children
        }

        /// <summary>
        /// Removes a child node from the DOM. Returns removed node.
        /// </summary>
        /// <param name="child">Child node to be removed from the DOM.</param>
        /// <returns>Returns the removed node.</returns>
        /// <exception cref="NotFoundException">If <paramref name="child"/>'s <see cref="Node.ParentNode"/> is not this node.</exception>
        public virtual Dom.Node RemoveChild(Dom.Node child)
        {
            // If child's parent is not parent, throw a "NotFoundError" exception.
            // Well, it cannot be here in the base class. See overrides.
            throw new Dom.Exceptions.NotFoundException();
        }

        /// <summary>
        /// Replaces one child node of the specified node with another.
        /// </summary>
        /// <param name="newChild">The new node to replace existingChild. If it already exists in the DOM, it is first removed.</param>
        /// <param name="existingChild">The existing child to be replaced</param>
        /// <returns>The replaced node. This is the same node as oldChild.</returns>
        public virtual Dom.Node ReplaceChild(Dom.Node newChild, Dom.Node existingChild)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-node-replace
            // To replace a child with node within a parent, run these steps:

            // 1. If parent is not a Document, DocumentFragment, or Element node, throw a "HierarchyRequestError".
            throw new Dom.Exceptions.HierarchyRequestException();
        }

        #endregion

        #region Internal extensions

        internal void SetParent(ParentNode parent, int nodeCollectionIndex)
        {
            this._ParentNode = parent;
            this.NodeCollectionIndex = (parent != null) ? nodeCollectionIndex : Node.NotAttachedToParentIndex;
        }

        internal Node SetCollectionIndex(int nodeCollectionIndex)
        {
            this.NodeCollectionIndex = nodeCollectionIndex;
            return this;
        }

        internal Node RootNode
        {
            get
            {
                Node node = this;
                while (node._ParentNode != null)
                {
                    node = node._ParentNode;
                }

                return node;
            }
        }

        internal void Adopt(Document document)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-node-adopt
            // To adopt a node into a document, run these steps:
            if (this is Document)
                throw new Dom.Exceptions.NotSupportedException();

            // 1. Let oldDocument be node's node document.
            Document oldDocument = this.Document;

            // 2. If node's parent is not null, remove node from its parent.
            this.ParentNode?.RemoveChild(this);

            // 3. Set node's inclusive descendants's node document to document.
            this.SetOwnerDocumentRecursive(document);

            // 4. Run any adopting steps defined for node in other applicable
            // specifications and pass node and oldDocument as parameters.
            this.RunAdoptingSteps(oldDocument);
        }

        protected virtual void RunAdoptingSteps(Dom.Document oldDocument)
        {
            // Override of needed
        }

        private void SetOwnerDocumentRecursive(Document document)
        {
            if (this is Document)
            {
                Contract.RequiresNull(document, nameof(document));
                return;
            }

            if (this.OwnerDocument == document)
                return; // Already adopted.

            this.OwnerDocument = document;

            foreach (Node child in this.ChildNodes)
                child.SetOwnerDocumentRecursive(document);
        }

        protected internal virtual void RunInsertionSteps()
        {
        }

        protected internal virtual void RunRemovingSteps(ParentNode parent, Dom.Node oldPreviousSibling)
        {
        }

        internal Document Document
        {
            get { return (Document)this.OwnerDocument ?? (this as Document); }
        }

        #endregion
    }
}

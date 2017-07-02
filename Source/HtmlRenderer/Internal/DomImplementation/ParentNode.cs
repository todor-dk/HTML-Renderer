using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal abstract class ParentNode : Node, Dom.ParentNode, Dom.NodeList, Dom.HtmlCollection
    {
        // This contains the child nodes and elements. See ElementChildren for further description.
        protected ElementChildren _Children = new ElementChildren();

        public ParentNode(Document document)
            : base(document)
        {
        }

        #region Node interface overrides

        /// <summary>
        /// Returns a live collection of child nodes of the given element where the first child node is assigned index 0.
        /// </summary>
        /// <remarks>
        /// <see cref="ChildNodes"/> includes all direct child nodes, including non-element nodes like text and comment nodes.
        /// To get a collection of only elements, use <see cref="ParentNode.Children"/> instead.
        /// </remarks>
        public override NodeList ChildNodes
        {
            get { return this; }
        }

        /// <summary>
        /// Returns the node's first child in the tree, or null if the node is childless. If the node is a Document,
        /// it returns the first node in the list of its direct children.
        /// </summary>
        public override Dom.Node FirstChild
        {
            get { return this._Children.GetNode(0); }
        }

        /// <summary>
        /// Returns the last child of the node. If its parent is an element, then the child is generally an element node,
        /// a text node, or a comment node. It returns null if there are no child elements.
        /// </summary>
        public override Dom.Node LastChild
        {
            get { return this._Children.GetNode(this._Children.GetNodeCount() - 1); }
        }

        /// <summary>
        /// Returns a bool value indicating whether the current Node has child nodes or not.
        /// </summary>
        /// <returns>True of the current node has children, otherwise false.</returns>
        public override bool HasChildren()
        {
            return this._Children.HasNodes();
        }

        /// <summary>
        /// Puts the node and all of its sub-tree into a "normalized" form. In a normalized sub-tree,
        /// no text nodes in the sub-tree are empty and there are no adjacent text nodes.
        /// </summary>
        public override void Normalize()
        {
            throw new NotImplementedException();
#if NOT_IMPLEMENTED
            // For each Text node descendant of the context object:
            if (!this._Children.HasNodes())
                return;
            Text[] texts = this.ChildNodes.OfType<Text>().ToArray();
            if (texts.Length == 0)
                return;
            foreach (Text node in texts)
            {
                // 1. Let node be the Text node descendant.

                // 2. Let length be node's length attribute value.
                int length = node.Length;

                // 3. If length is zero, remove node and continue with the next Text node, if any.
                if (length == 0)
                {
                    this.RemoveNode(node, false);
                    continue;
                }

                // 4. Let data be the concatenation of the data of node's contiguous Text nodes (excluding itself), in tree order.
                // 5. Replace data with node node, offset length, count 0, and data data.
                // 6. Let current node be node's next sibling.
                // 7. While current node is a Text node:
                    // 1. For each range whose start node is current node, add length to its start offset and set its start node to node.
                    // 2. For each range whose end node is current node, add length to its end offset and set its end node to node.
                    // 3. For each range whose start node is current node's parent and start offset is current node's index, set its start node to node and its start offset to length.
                    // 4. For each range whose end node is current node's parent and end offset is current node's index, set its end node to node and its end offset to length.
                    // 5. Add current node's length attribute value to length.
                    // 6. Set current node to its next sibling.
                // 8. Remove node's contiguous Text nodes (excluding itself), in tree order.
            }
#endif
        }

        /// <summary>
        /// Removes a child node from the DOM. Returns removed node.
        /// </summary>
        /// <param name="child">Child node to be removed from the DOM.</param>
        /// <returns>Returns the removed node.</returns>
        /// <exception cref="NotFoundException">If <paramref name="child"/>'s <see cref="Node.ParentNode"/> is not this node.</exception>
        public override Dom.Node RemoveChild(Dom.Node child)
        {
            Contract.RequiresNotNull(child, nameof(child));

            return this.PreRemoveNode((Node)child);
        }

        /// <summary>
        /// Inserts the specified <paramref name="newChild"/> before the reference node as a child of the current node.
        /// </summary>
        /// <param name="newChild">New node to be added as a child to this node.</param>
        /// <param name="referenceChild">Reference child node. If referenceNode is null, the newNode is inserted at the end of the list of child nodes.</param>
        /// <returns>The inserted node.</returns>
        public override Dom.Node InsertBefore(Dom.Node newChild, Dom.Node referenceChild)
        {
            Contract.RequiresNotNull(newChild, nameof(newChild));

            return this.PreInsertNode((Node)newChild, (Node)referenceChild);
        }

        /// <summary>
        /// Adds <paramref name="node"/> to the end of the list of children of this node. If the given child is
        /// an existing node in the document, AppendChild moves it from its current position to the new position
        /// (there is no requirement to remove the node from its parent node before appending it to some other node).
        /// </summary>
        /// <param name="node">Node to be added.</param>
        /// <returns>The appended child.</returns>
        public override Dom.Node AppendChild(Dom.Node node)
        {
            Contract.RequiresNotNull(node, nameof(node));

            return this.PreInsertNode((Node)node, null);
        }

        /// <summary>
        /// Replaces one child node of the specified node with another.
        /// </summary>
        /// <param name="newChild">The new node to replace existingChild. If it already exists in the DOM, it is first removed.</param>
        /// <param name="existingChild">The existing child to be replaced</param>
        /// <returns>The replaced node. This is the same node as oldChild.</returns>
        public override Dom.Node ReplaceChild(Dom.Node newChild, Dom.Node existingChild)
        {
            Contract.RequiresNotNull(newChild, nameof(newChild));
            Contract.RequiresNotNull(existingChild, nameof(existingChild));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-node-replace
            // To replace a child with node within a parent, run these steps:
            Node node = (Node)newChild;
            Node child = (Node)existingChild;

            // 1. If parent is not a Document, DocumentFragment, or Element node, throw a "HierarchyRequestError".
            // It is!

            // 2. If node is a host-including inclusive ancestor of parent, throw a "HierarchyRequestError".
            if (node.IsHostIncludingInclusiveAncestor(this))
                throw new Dom.Exceptions.HierarchyRequestException();

            // 3. If child's parent is not parent, throw a "NotFoundError" exception.
            if ((child != null) && (child.ParentNode != this))
                throw new Dom.Exceptions.NotFoundException();

            // 4. If node is not a DocumentFragment, DocumentType, Element, Text, ProcessingInstruction, or Comment node, throw a "HierarchyRequestError".
            if (!((node is DocumentFragment) || (node is DocumentType) || (node is Element) || (node is Text) || (node is Comment)))
                throw new Dom.Exceptions.HierarchyRequestException();

            // 5. If either node is a Text node and parent is a document, or node is a doctype and parent is not a document, throw a "HierarchyRequestError".
            if (((node is Text) && (this is Document)) || ((node is DocumentType) && !(this is Document)))
                throw new Dom.Exceptions.HierarchyRequestException();

            // 6. If parent is a document, and any of the statements below, switched on node, are true, throw a "HierarchyRequestError".
            if (this is Document)
            {
                Document doc = (Document)this;

                // Note: The above statements differ from the pre-insert algorithm.

                // * DocumentFragment node
                if (node is DocumentFragment)
                {
                    DocumentFragment df = (DocumentFragment)node;

                    // If node has more than one element child or has a Text node child.
                    if ((df.ChildElementCount > 1) || df.ChildNodes.Any(e => e is Text))
                        throw new Dom.Exceptions.HierarchyRequestException();

                    // Otherwise, if node has one element child and either parent has an element child
                    // that is not child or a doctype is following child.
                    if ((df.ChildElementCount == 1) && ((doc.ChildElementCount > 0) && (!doc.Children.Contains((Dom.Element)child) || (child.NextSibling is DocumentType))))
                        throw new Dom.Exceptions.HierarchyRequestException();
                }

                // * element
                else if (node is Element)
                {
                    // parent has an element child that is not child or a doctype is following child.
                    if (((doc.ChildElementCount > 0) && !doc.Children.Contains((Dom.Element)child)) || (child.NextSibling is DocumentType))
                        throw new Dom.Exceptions.HierarchyRequestException();
                }

                // * doctype
                else if (node is DocumentType)
                {
                    // parent has a doctype child that is not child, or an element is preceding child.
                    if (this.ChildNodes.Any(e => e is DocumentType) || (child.PreviousSibling is Element))
                        throw new Dom.Exceptions.HierarchyRequestException();
                }
            }

            // 7. Let reference child be child's next sibling.
            Dom.Node referenceChild = child.NextSibling;

            // 8. If reference child is node, set it to node's next sibling.
            if (referenceChild == node)
                referenceChild = node.NextSibling;

            // 9. Adopt node into parent's node document.
            this.Document.AdoptNode(node);

            // 10. Remove child from its parent with the suppress observers flag set.
            this._Children.RemoveNode(this, child, true);

            // 11. Insert node into parent before reference child with the suppress observers flag set.
            this._Children.InsertNode(this, node, (Node)referenceChild, true);

            // 12. Let nodes be node's children if node is a DocumentFragment node, and a list containing solely node otherwise.
            Node[] nodes = (node is DocumentFragment) ? ((DocumentFragment)node).ChildNodes.Cast<Node>().ToArray() : new Node[] { node };

            // 13. Queue a mutation record of "childList" for target parent with addedNodes nodes, removedNodes a list solely containing child, nextSibling reference child, and previousSibling child's previous sibling.
            // TODO: Implement observers

            // 14. Return child.
            return child;
        }

        #endregion

        #region ParentNode interface

        /// <summary>
        /// Returns the number of children of the context object that are elements.
        /// </summary>
        public int ChildElementCount
        {
            get { return this._Children.GetElementCount(); }
        }

        /// <summary>
        /// Returns the child elements.
        /// The children attribute returns an HTMLCollection collection rooted at the context object matching only element children.
        /// </summary>
        public HtmlCollection Children
        {
            get { return this; }
        }

        /// <summary>
        /// Returns the first child that is an element, and null otherwise.
        /// </summary>
        public Dom.Element FirstElementChild
        {
            get { return this._Children.GetElement(0); }
        }

        /// <summary>
        /// Returns the last child that is an element, and null otherwise.
        /// </summary>
        public Dom.Element LastElementChild
        {
            get { return this._Children.GetLastElement(); }
        }

        #endregion

        #region NodeList interface

        /*
            To optimize memory, instead of using a separate collection object,
            THIS object is the same as the child collection object.
        */

        /// <summary>
        /// Returns the node with index index from the collection. The nodes are sorted in tree order.
        /// </summary>
        /// <param name="index">The requested index.</param>
        /// <returns>Returns the node with index index from the collection or null if there is no index'th node in the collection.</returns>
        Dom.Node NodeList.Item(int index)
        {
            return this._Children.GetNode(index);
        }

        /// <summary>
        /// Returns the number of nodes in the collection.
        /// </summary>
        int NodeList.Length
        {
            get { return this._Children.GetNodeCount(); }
        }

        /// <summary>
        /// Gets the number of nodes in the collection.
        /// </summary>
        int IReadOnlyCollection<Dom.Node>.Count
        {
            get { return this._Children.GetNodeCount(); }
        }

        /// <summary>
        /// Gets the node at the specified index in the child node list.
        /// </summary>
        /// <param name="index">The zero-based index of the node to get.</param>
        /// <returns>The node at the specified index in the list.</returns>
        Dom.Node IReadOnlyList<Dom.Node>.this[int index]
        {
            get
            {
                Dom.Node node = this.GetNode(index);
                if (node == null)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return node;
            }
        }

        IEnumerator<Dom.Node> IEnumerable<Dom.Node>.GetEnumerator()
        {
            return this._Children.GetNodeEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._Children.GetNodeEnumerator();
        }

        #endregion

        #region HtmlCollection interface

        /// <summary>
        /// Returns the element with index index from the collection. The elements are sorted in tree order.
        /// </summary>
        /// <param name="index">The requested index.</param>
        /// <returns>Returns the element with index index from the collection or null if there is no index'th element in the collection.</returns>
        Dom.Element HtmlCollection.Item(int index)
        {
            return this._Children.GetElement(index);
        }

        /// <summary>
        /// Returns the first element with ID or name name from the collection.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Dom.Element HtmlCollection.NamedItem(string name)
        {
            Contract.RequiresNotNull(name, nameof(name));

            return this._Children.GetNamedElement(name);
        }

        /// <summary>
        /// Returns the number of elements in the collection.
        /// </summary>
        int HtmlCollection.Length
        {
            get { return this._Children.GetElementCount(); }
        }

        Dom.Element IReadOnlyList<Dom.Element>.this[int index]
        {
            get
            {
                Dom.Element element = this._Children.GetElement(index);
                if (element == null)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return element;
            }
        }

        int IReadOnlyCollection<Dom.Element>.Count
        {
            get { return this._Children.GetElementCount(); }
        }

        IEnumerator<Dom.Element> IEnumerable<Dom.Element>.GetEnumerator()
        {
            return this._Children.GetElementEnumerator();
        }

        #endregion

        #region Internal extensions

        internal Node GetNode(int index)
        {
            return this._Children.GetNode(index);
        }

        internal Node PreInsertNode(Node node, Node child)
        {
            Contract.RequiresNotNull(node, nameof(node));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-node-pre-insert
            // To pre-insert a node into a parent before a child, run these steps:

            // 1. Ensure pre-insertion validity of node into parent before child.
            this.EnsurePreInsertionValidity(node, child);

            // 2. Let reference child be child.
            Node referenceChild = child;

            // 3. If reference child is node, set it to node's next sibling.
            if (referenceChild == node)
                referenceChild = (Node)node.NextSibling;

            // 4. Adopt node into parent's node document.
            this.Document.AdoptNode(node);

            // 5. Insert node into parent before reference child.
            this._Children.InsertNode(this, node, referenceChild, false);

            // 6. Return node.
            return node;
        }

        internal Node PreRemoveNode(Node child)
        {
            Contract.RequiresNotNull(child, nameof(child));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-node-pre-remove
            // To pre-remove a child from a parent, run these steps:

            // 1. If child's parent is not parent, throw a "NotFoundError" exception.
            if (child.ParentNode != this)
                throw new Dom.Exceptions.NotFoundException();

            // 2. Remove child from parent.
            this._Children.RemoveNode(this, child, false);

            // 3. Return child.
            return child;
        }

        internal void RemoveAllChildNodes()
        {
            foreach (Node child in this.ChildNodes.ToArray())
                this._Children.RemoveNode(this, child, true);
        }

        internal void RemoveNode(Node child)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-node-remove
            // NB: At this point, we expect that arguments are validated and legal.
            this._Children.RemoveNode(this, child, false);
        }

        internal Element GetNextElementSibling(Node node)
        {
            return this._Children.GetNextElementSibling(node);
        }

        internal Element GetPreviousElementSibling(Node node)
        {
            return this._Children.GetPreviousElementSibling(node);
        }

        #endregion
    }
}

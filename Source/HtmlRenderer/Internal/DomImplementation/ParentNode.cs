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
        public ParentNode(Document document)
            : base(document)
        {
        }

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
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns the last child of the node. If its parent is an element, then the child is generally an element node,
        /// a text node, or a comment node. It returns null if there are no child elements.
        /// </summary>
        public override Dom.Node LastChild
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns a bool value indicating whether the current Node has child nodes or not.
        /// </summary>
        /// <returns>True of the current node has children, otherwise false.</returns>
        public override bool HasChildren()
        {
            return base.HasChildren();
        }

        public int ChildElementCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public HtmlCollection Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dom.Element FirstElementChild
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dom.Element LastElementChild
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        #region NodeList interface

        public Node GetNode(int index)
        {
            throw new NotImplementedException();
        }

        Dom.Node NodeList.Item(int index)
        {
            return this.GetNode(index);
        }

        int NodeList.Length
        {
            get { return 0; }
        }

        int IReadOnlyCollection<Dom.Node>.Count
        {
            get { return 0; }
        }

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
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region HtmlCollection interface

        public Element GetElement(int index)
        {
            throw new NotImplementedException();
        }

        Dom.Element HtmlCollection.Item(int index)
        {
            return this.GetElement(index);
        }

        Dom.Element HtmlCollection.NamedItem(string name)
        {
            return null;
        }

        int HtmlCollection.Length
        {
            get { return 0; }
        }

        Dom.Element IReadOnlyList<Dom.Element>.this[int index]
        {
            get
            {
                Dom.Element element = this.GetElement(index);
                if (element == null)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return element;
            }
        }

        int IReadOnlyCollection<Dom.Element>.Count
        {
            get { return 0; }
        }

        IEnumerator<Dom.Element> IEnumerable<Dom.Element>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

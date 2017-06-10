using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class Element : ParentNode, Dom.Element, Dom.AttrCollection
    {
        // This contains the attributes of the element. See ElementAttributes for further description.
        private readonly ElementAttributes _Attributes = new ElementAttributes();

        public Element(Document document)
            : base(document)
        {
        }

        #region Node interface overrides

        /// <summary>
        /// Returns a string appropriate for the type of node.
        /// </summary>
        public override string NodeName
        {
            get { return this.TagName; }
        }

        /// <summary>
        /// Returns the type of node.
        /// </summary>
        public override NodeType NodeType
        {
            get { return NodeType.Element; }
        }

        /// <summary>
        /// Returns a duplicate of this node.
        /// </summary>
        /// <param name="deep">True if the children of the node should also be cloned, or false to clone only the specified node.</param>
        /// <returns>A new node that is a clone this node.</returns>
        public override Dom.Node CloneNode(bool deep = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The text content of a node and its descendants.
        /// </summary>
        public override string TextContent
        {
            get { return this.GetTextContent(); }
            set { this.SetTextContent(value); }
        }

        #endregion

        #region ChildNode interface

        /// <summary>
        /// Removes this node from its parent children list.
        /// </summary>
        public void Remove()
        {
            this._ParentNode?.RemoveNode(this);
        }

        #endregion

        #region NonDocumentTypeChildNode interface

        /// <summary>
        /// Returns the first following sibling that is an element, and null otherwise.
        /// </summary>
        public Dom.Element NextElementSibling
        {
            get { return this._ParentNode?.GetNextElementSibling(this); }
        }

        /// <summary>
        /// Returns the first preceding sibling that is an element, and null otherwise.
        /// </summary>
        public Dom.Element PreviousElementSibling
        {
            get { return this._ParentNode.GetPreviousElementSibling(this); }
        }

        #endregion

        #region Element interface

        public AttrCollection Attributes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DomTokenList ClassList
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ClassName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string LocalName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string NamespaceUri
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Prefix
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string TagName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string GetAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public string GetAttributeNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByClassName(string classNames)
        {
            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByTagName(string localName)
        {
            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByTagNameNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        public bool HasAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public bool HasAttributeNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        public void RemoveAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveAttributeNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        public void SetAttribute(string name, string value)
        {
            throw new NotImplementedException();
        }

        public void SetAttributeNS(string @namespace, string name, string value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region AttrCollection interface

        int AttrCollection.Count
        {
            get { return this._Attributes.GetCount(); }
        }

        int NamedNodeMap.Length
        {
            get { return this._Attributes.GetCount(); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._Attributes.GetEnumerator();
        }

        IEnumerator<Dom.Attr> IEnumerable<Dom.Attr>.GetEnumerator()
        {
            return this._Attributes.GetEnumerator();
        }

        Dom.Node NamedNodeMap.GetNamedItem(string name)
        {
            throw new NotImplementedException();
        }

        Dom.Node NamedNodeMap.GetNamedItemNS(string namespaceUri, string localName)
        {
            throw new NotImplementedException();
        }

        Dom.Node NamedNodeMap.Item(int index)
        {
            throw new NotImplementedException();
        }

        Dom.Node NamedNodeMap.RemoveNamedItem(string name)
        {
            throw new NotImplementedException();
        }

        Dom.Node NamedNodeMap.RemoveNamedItemNS(string namespaceUri, string localName)
        {
            throw new NotImplementedException();
        }

        Dom.Node NamedNodeMap.SetNamedItem(Dom.Node arg)
        {
            throw new NotImplementedException();
        }

        Dom.Node NamedNodeMap.SetNamedItemNS(Dom.Node arg)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Internal extensions

        internal bool IsNamed(string name)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

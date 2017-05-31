using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class Element : ParentNode, Dom.Element
    {
        public Element(Document document)
            : base(document)
        {
        }

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

        public Dom.Element NextElementSibling
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

        public string Prefix
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dom.Element PreviousElementSibling
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

        /// <summary>
        /// The text content of a node and its descendants.
        /// </summary>
        public override string TextContent
        {
            get { return this.GetTextContent(); }
            set { this.SetTextContent(value); }
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

        public void Remove()
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
    }
}

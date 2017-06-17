using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public abstract class ReferenceNode
    {
        public ReferenceNode()
        {
            this.ChildNodes = new ReferenceNodeList();
        }

        public ReferenceNode(Persisting.IReader reader, NodeType type)
            : this()
        {
            this.NodeType = type;
            this.NodeName = reader.ReadString("NodeName");
            this.BaseUri = reader.ReadString("BaseUri");

            reader.ReadNode("OwnerDocument", node => this.OwnerDocument = (ReferenceDocument)node);
            reader.ReadNode("ParentNode", node => this.ParentNode = node);
            reader.ReadElement("ParentElement", elem => this.ParentElement = elem);
            reader.ReadNode("FirstChild", node => this.FirstChild = node);
            reader.ReadNode("LastChild", node => this.LastChild = node);
            reader.ReadNode("PreviousSibling", node => this.PreviousSibling = node);
            reader.ReadNode("NextSibling", node => this.NextSibling = node);
            this.NodeValue = reader.ReadString("NodeValue");
            this.TextContent = reader.ReadString("TextContent");
            reader.ReadNodeList("ChildNodes", list => this.ChildNodes.AddRange(list));
        }

        public NodeType NodeType { get; private set; }

        public string NodeName { get; private set; }
        
        public string BaseUri { get; private set; }

        public ReferenceDocument OwnerDocument { get; private set; }

        public ReferenceNode ParentNode { get; private set; }

        public ReferenceElement ParentElement { get; private set; }
        
        public ReferenceNodeList ChildNodes { get; private set; }

        public ReferenceNode FirstChild { get; private set; }

        public ReferenceNode LastChild { get; private set; }

        public ReferenceNode PreviousSibling { get; private set; }

        public ReferenceNode NextSibling { get; private set; }

        public string NodeValue { get; private set; }

        public string TextContent { get; private set; }

        public void Accept(IGenericVisitor visitor)
        {
            visitor.Visit(this);

            this.FirstChild?.Accept(visitor);
            this.NextSibling?.Accept(visitor);
        }

        public void Accept(IConcreteVisitor visitor)
        {
            this.AcceptOverride(visitor);

            this.FirstChild?.Accept(visitor);
            this.NextSibling?.Accept(visitor);
        }

        protected abstract void AcceptOverride(IConcreteVisitor visitor);

        public abstract bool CompareWith(ReferenceNode other, CompareContext context);

        internal bool CompareWithNode(ReferenceNode other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (this.BaseUri != other.BaseUri)
                return false;
            if (!this.ChildNodes.CompareReferenceCollection(other.ChildNodes, context))
                return false;
            if (!this.FirstChild.CompareReference(other.FirstChild, context))
                return false;
            if (!this.LastChild.CompareReference(other.LastChild, context))
                return false;
            if (!this.NextSibling.CompareReference(other.NextSibling, context))
                return false;
            if (this.NodeName != other.NodeName)
                return false;
            if (this.NodeType != other.NodeType)
                return false;
            if (this.NodeValue != other.NodeValue)
                return false;
            if (!this.OwnerDocument.CompareReference(other.OwnerDocument, context))
                return false;
            if (!this.ParentElement.CompareReference(other.ParentElement, context))
                return false;
            if (!this.ParentNode.CompareReference(other.ParentNode, context))
                return false;
            if (!this.PreviousSibling.CompareReference(other.PreviousSibling, context))
                return false;
            if (this.TextContent != other.TextContent)
                return false;

            return true;
        }

        public abstract bool CompareWith(Node other, CompareContext context);

        internal bool CompareWithNode(Node other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if ((this.BaseUri != other.BaseUri) && !context.IgnoreBaseUriExceptForElementAndDocument && !((this is ReferenceElement) || (this is ReferenceDocument)))
                return false;
                if (!this.ChildNodes.CompareDomCollection(other.ChildNodes, context))
                return false;
            if (!this.FirstChild.CompareDom(other.FirstChild, context))
                return false;
            if (!this.LastChild.CompareDom(other.LastChild, context))
                return false;
            if (!this.NextSibling.CompareDom(other.NextSibling, context))
                return false;
            if (this.NodeName != other.NodeName)
                return false;
            if (this.NodeType != other.NodeType)
                return false;
            if (this.NodeValue != other.NodeValue)
                return false;
            if (!this.OwnerDocument.CompareDom(other.OwnerDocument, context))
                return false;
            if (!this.ParentElement.CompareDom(other.ParentElement, context))
                return false;
            if (!this.ParentNode.CompareDom(other.ParentNode, context))
                return false;
            if (!this.PreviousSibling.CompareDom(other.PreviousSibling, context))
                return false;
            if (this.TextContent != other.TextContent)
                return false;

            return true;
        }
    }
}

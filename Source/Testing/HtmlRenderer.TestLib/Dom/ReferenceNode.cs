using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public abstract class ReferenceNode : Node
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
        
        #region Node interface

        Document Node.OwnerDocument => this.OwnerDocument;

        Node Node.ParentNode => this.ParentNode;

        Element Node.ParentElement => this.ParentElement;

        NodeList Node.ChildNodes => this.ChildNodes;

        Node Node.FirstChild => this.FirstChild;

        Node Node.LastChild => this.LastChild;

        Node Node.PreviousSibling => this.PreviousSibling;

        Node Node.NextSibling => this.NextSibling;

        string Node.NodeValue { get => this.NodeValue; set => throw new NotImplementedException(); }
        string Node.TextContent { get => this.TextContent; set => throw new NotImplementedException(); }

        bool Node.HasChildren()
        {
            return this.ChildNodes.Count != 0;
        }

        void Node.Normalize()
        {
            throw new NotImplementedException();
        }

        Node Node.CloneNode(bool deep)
        {
            throw new NotImplementedException();
        }

        DocumentPosition Node.CompareDocumentPosition(Node other)
        {
            throw new NotImplementedException();
        }

        bool Node.Contains(Node other)
        {
            if (other == null)
                return false;
            return (this == other) || other.IsDescendantOf(this);
        }

        string Node.LookupPrefix(string @namespace)
        {
            throw new NotImplementedException();
        }

        string Node.LookupNamespaceUri(string prefix)
        {
            throw new NotImplementedException();
        }

        bool Node.IsDefaultNamespace(string @namespace)
        {
            throw new NotImplementedException();
        }

        Node Node.InsertBefore(Node newChild, Node referenceChild)
        {
            throw new NotImplementedException();
        }

        Node Node.AppendChild(Node node)
        {
            throw new NotImplementedException();
        }

        Node Node.ReplaceChild(Node newChild, Node existingChild)
        {
            throw new NotImplementedException();
        }

        Node Node.RemoveChild(Node child)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}

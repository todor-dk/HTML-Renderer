using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2000/REC-DOM-Level-2-Core-20001113/core.html#ID-1950641247
    public abstract class Node
    {
        public abstract string NodeName { get; }

        public abstract string NodeValue { get; }

        public abstract NodeType NodeType { get; }

        public abstract Node ParentNode { get; }

        public abstract IReadOnlyList<Node> ChildNodes { get; }

        public abstract Node FirstChild { get; }

        public abstract Node LastChild { get; }

        public abstract Node PreviousSibling { get; }

        public abstract Node NextSibling { get; }

        public abstract IReadOnlyList<Attr> Attributes { get; }

        public Document OwnerDocument { get; private set; }

        public Node(Document ownerDocument)
        {
            Contract.RequiresNotNull(ownerDocument, nameof(ownerDocument));
            this.OwnerDocument = ownerDocument;
        }

        public Node InsertBefore(Node newChild, Node referenceChild)
        {
            throw new NotImplementedException();
        }

        public Node ReplaceChild(Node newChild, Node oldChild)
        {
            throw new NotImplementedException();
        }

        public Node RemoveChild(Node oldChild)
        {
            throw new NotImplementedException();
        }

        public Node AppendChild(Node newChild)
        {
            throw new NotImplementedException();
        }

        public bool HasChildNodes()
        {
            return this.ChildNodes.Count != 0;
        }

        public void Normalize()
        {
            throw new NotImplementedException();
        }
    }
}
